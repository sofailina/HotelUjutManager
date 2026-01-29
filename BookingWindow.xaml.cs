using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace HotelUjutManager
{
    public partial class BookingWindow : Window
    {
        private string connectionString = "Data Source=S\\STP;Initial Catalog=HotelUjut;Integrated Security=True";

        public BookingWindow()
        {
            InitializeComponent();
            Loaded += BookingWindow_Loaded;
        }

        private void BookingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGuests();
            LoadRooms();
            LoadBookings();
        }

        private void LoadGuests()
        {
            try
            {
                CmbGuests.Items.Clear();

                string query = "SELECT id, full_name FROM Guest ORDER BY full_name";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                           
                            var item = new ComboBoxItem
                            {
                                Content = reader["full_name"].ToString(),
                                Tag = reader["id"]
                            };
                            CmbGuests.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки гостей: {ex.Message}");
            }
        }

        private void LoadRooms()
        {
            try
            {
                CmbRooms.Items.Clear();

                string query = @"
            SELECT 
                r.id,
                r.room_number,
                rt.name as room_type,
                rt.price_per_night
            FROM Room r
            LEFT JOIN RoomType rt ON r.room_type_id = rt.id
            ORDER BY r.room_number";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string price = reader["price_per_night"] != DBNull.Value ?
                                reader["price_per_night"].ToString() : "0";
                            string roomType = reader["room_type"] != DBNull.Value ?
                                reader["room_type"].ToString() : "Не указан";

                            string roomInfo = $"{reader["room_number"]} - {roomType} ({price} руб./ночь)";
                            var item = new ComboBoxItem
                            {
                                Content = roomInfo,
                                Tag = reader["id"]
                            };
                            CmbRooms.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки номеров: {ex.Message}");

                CmbRooms.Items.Add(new ComboBoxItem { Content = "101 - Стандарт (2520 руб./ночь)", Tag = 1 });
                CmbRooms.Items.Add(new ComboBoxItem { Content = "102 - Стандарт (2520 руб./ночь)", Tag = 2 });
                CmbRooms.Items.Add(new ComboBoxItem { Content = "201 - Бизнес (5000 руб./ночь)", Tag = 3 });
                CmbRooms.Items.Add(new ComboBoxItem { Content = "304 - Люкс (8000 руб./ночь)", Tag = 4 });
            }
        }

        private void LoadBookings()
        {
            try
            {

                string query = @"
                    SELECT 
                        b.id,
                        g.full_name as guest_name,
                        r.room_number,
                        b.check_in,
                        b.check_out,
                        b.status
                    FROM Booking b
                    JOIN Guest g ON b.guest_id = g.id
                    JOIN Room r ON b.room_id = r.id
                    ORDER BY b.check_in DESC";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var adapter = new SqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table);

                    if (table.Columns.Count > 0)
                    {
                        table.Columns["id"].ColumnName = "ID";
                        table.Columns["guest_name"].ColumnName = "Гость";
                        table.Columns["room_number"].ColumnName = "Номер";
                        table.Columns["check_in"].ColumnName = "Заезд";
                        table.Columns["check_out"].ColumnName = "Выезд";
                        table.Columns["status"].ColumnName = "Статус";
                    }

                    DgBookings.ItemsSource = table.DefaultView;

                    MessageBox.Show($"Загружено бронирований: {table.Rows.Count}", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка SQL при загрузке бронирований: {ex.Message}\n" +
                    "Убедитесь, что таблицы Booking, Guest и Room существуют.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CmbGuests.SelectedItem == null)
            {
                MessageBox.Show("Выберите гостя!");
                return;
            }

            if (CmbRooms.SelectedItem == null)
            {
                MessageBox.Show("Выберите номер!");
                return;
            }

            if (!DpCheckIn.SelectedDate.HasValue || !DpCheckOut.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите даты заезда и выезда!");
                return;
            }

            if (DpCheckIn.SelectedDate.Value >= DpCheckOut.SelectedDate.Value)
            {
                MessageBox.Show("Дата выезда должна быть позже даты заезда!");
                return;
            }

            try
            {
                int guestId = (int)((ComboBoxItem)CmbGuests.SelectedItem).Tag;
                int roomId = (int)((ComboBoxItem)CmbRooms.SelectedItem).Tag;
                string status = ((ComboBoxItem)CmbStatus.SelectedItem).Content.ToString();

                string query = @"
                    INSERT INTO Booking (guest_id, room_id, check_in, check_out, status) 
                    VALUES (@guest_id, @room_id, @check_in, @check_out, @status)";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@guest_id", guestId);
                    command.Parameters.AddWithValue("@room_id", roomId);
                    command.Parameters.AddWithValue("@check_in", DpCheckIn.SelectedDate.Value);
                    command.Parameters.AddWithValue("@check_out", DpCheckOut.SelectedDate.Value);
                    command.Parameters.AddWithValue("@status", status);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Бронирование успешно добавлено!");
                        LoadBookings();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления бронирования: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (DgBookings.SelectedItem == null)
            {
                MessageBox.Show("Выберите бронирование для редактирования!");
                return;
            }

            if (CmbGuests.SelectedItem == null || CmbRooms.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            try
            {
                var rowView = (DataRowView)DgBookings.SelectedItem;
                int bookingId = Convert.ToInt32(rowView["ID"]);

                int guestId = (int)((ComboBoxItem)CmbGuests.SelectedItem).Tag;
                int roomId = (int)((ComboBoxItem)CmbRooms.SelectedItem).Tag;
                string status = ((ComboBoxItem)CmbStatus.SelectedItem).Content.ToString();

                string query = @"
                    UPDATE Booking 
                    SET guest_id = @guest_id, 
                        room_id = @room_id, 
                        check_in = @check_in, 
                        check_out = @check_out, 
                        status = @status 
                    WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", bookingId);
                    command.Parameters.AddWithValue("@guest_id", guestId);
                    command.Parameters.AddWithValue("@room_id", roomId);
                    command.Parameters.AddWithValue("@check_in", DpCheckIn.SelectedDate.Value);
                    command.Parameters.AddWithValue("@check_out", DpCheckOut.SelectedDate.Value);
                    command.Parameters.AddWithValue("@status", status);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Бронирование обновлено!");
                        LoadBookings();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgBookings.SelectedItem == null)
            {
                MessageBox.Show("Выберите бронирование для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранное бронирование?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var rowView = (DataRowView)DgBookings.SelectedItem;
                int bookingId = Convert.ToInt32(rowView["ID"]);

                string query = "DELETE FROM Booking WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", bookingId);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Бронирование удалено!");
                        LoadBookings();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadGuests();
            LoadRooms();
            LoadBookings();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgBookings.SelectedItem is DataRowView rowView)
            {
                string guestName = rowView["Гость"].ToString();
                foreach (ComboBoxItem item in CmbGuests.Items)
                {
                    if (item.Content.ToString() == guestName)
                    {
                        CmbGuests.SelectedItem = item;
                        break;
                    }
                }

                string roomInfo = rowView["Номер"].ToString();
                foreach (ComboBoxItem item in CmbRooms.Items)
                {
                    if (item.Content.ToString().Contains(roomInfo))
                    {
                        CmbRooms.SelectedItem = item;
                        break;
                    }
                }

                if (rowView["Заезд"] != DBNull.Value)
                    DpCheckIn.SelectedDate = Convert.ToDateTime(rowView["Заезд"]);

                if (rowView["Выезд"] != DBNull.Value)
                    DpCheckOut.SelectedDate = Convert.ToDateTime(rowView["Выезд"]);

                string status = rowView["Статус"].ToString();
                foreach (ComboBoxItem item in CmbStatus.Items)
                {
                    if (item.Content.ToString() == status)
                    {
                        CmbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        }
        private void ClearForm()
        {
            CmbGuests.SelectedItem = null;
            CmbRooms.SelectedItem = null;
            DpCheckIn.SelectedDate = DateTime.Now;
            DpCheckOut.SelectedDate = DateTime.Now.AddDays(1);
            CmbStatus.SelectedIndex = 0;
            DgBookings.SelectedItem = null;
        }
    }
}