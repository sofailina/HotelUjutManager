using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace HotelUjutManager
{
    public partial class RoomsWindow : Window
    {
        private string connectionString = "Data Source=S\\STP;Initial Catalog=HotelUjut;Integrated Security=True";

        public RoomsWindow()
        {
            InitializeComponent();
            Loaded += RoomsWindow_Loaded;
        }

        private void RoomsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRoomTypes();
            LoadRooms();
        }

        private void LoadRoomTypes()
        {
            try
            {
                CmbRoomType.Items.Clear();

                string query = "SELECT id, name, price_per_night FROM RoomType ORDER BY name";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string typeInfo = $"{reader["name"]} ({reader["price_per_night"]} руб.)";
                            var item = new ComboBoxItem
                            {
                                Content = typeInfo,
                                Tag = reader["id"]
                            };
                            CmbRoomType.Items.Add(item);
                        }
                    }
                }
            }
            catch
            {
                CmbRoomType.Items.Add(new ComboBoxItem { Content = "Одноместный стандарт (2520 руб.)", Tag = 1 });
                CmbRoomType.Items.Add(new ComboBoxItem { Content = "Двуместный стандарт (3500 руб.)", Tag = 2 });
                CmbRoomType.Items.Add(new ComboBoxItem { Content = "Бизнес (5000 руб.)", Tag = 3 });
                CmbRoomType.Items.Add(new ComboBoxItem { Content = "Люкс (8000 руб.)", Tag = 4 });
            }
        }

        private void LoadRooms()
        {
            try
            {
                string query = @"
                    SELECT 
                        r.id,
                        r.room_number,
                        r.floor,
                        rt.name as room_type,
                        r.status,
                        rt.price_per_night
                    FROM Room r
                    LEFT JOIN RoomType rt ON r.room_type_id = rt.id
                    ORDER BY r.room_number";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var adapter = new SqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table);

                    if (table.Columns.Count > 0)
                    {
                        table.Columns["id"].ColumnName = "ID";
                        table.Columns["room_number"].ColumnName = "Номер";
                        table.Columns["floor"].ColumnName = "Этаж";
                        table.Columns["room_type"].ColumnName = "Тип номера";
                        table.Columns["status"].ColumnName = "Статус";
                        table.Columns["price_per_night"].ColumnName = "Цена за ночь";
                    }

                    DgRooms.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки номеров: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtRoomNumber.Text))
            {
                MessageBox.Show("Введите номер комнаты!");
                return;
            }

            if (CmbRoomType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип номера!");
                return;
            }

            try
            {
                int roomTypeId = (int)((ComboBoxItem)CmbRoomType.SelectedItem).Tag;
                string status = ((ComboBoxItem)CmbStatus.SelectedItem).Content.ToString();

                string query = @"
                    INSERT INTO Room (room_number, floor, room_type_id, status) 
                    VALUES (@room_number, @floor, @room_type_id, @status)";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@room_number", TxtRoomNumber.Text);
                    command.Parameters.AddWithValue("@floor", TxtFloor.Text ?? "");
                    command.Parameters.AddWithValue("@room_type_id", roomTypeId);
                    command.Parameters.AddWithValue("@status", status);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Номер успешно добавлен!");
                        LoadRooms();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления номера: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (DgRooms.SelectedItem == null)
            {
                MessageBox.Show("Выберите номер для редактирования!");
                return;
            }

            try
            {
                var rowView = (DataRowView)DgRooms.SelectedItem;
                int roomId = Convert.ToInt32(rowView["ID"]);

                int roomTypeId = (int)((ComboBoxItem)CmbRoomType.SelectedItem).Tag;
                string status = ((ComboBoxItem)CmbStatus.SelectedItem).Content.ToString();

                string query = @"
                    UPDATE Room 
                    SET room_number = @room_number, 
                        floor = @floor, 
                        room_type_id = @room_type_id, 
                        status = @status 
                    WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", roomId);
                    command.Parameters.AddWithValue("@room_number", TxtRoomNumber.Text);
                    command.Parameters.AddWithValue("@floor", TxtFloor.Text ?? "");
                    command.Parameters.AddWithValue("@room_type_id", roomTypeId);
                    command.Parameters.AddWithValue("@status", status);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Номер обновлен!");
                        LoadRooms();
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
            if (DgRooms.SelectedItem == null)
            {
                MessageBox.Show("Выберите номер для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранный номер?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var rowView = (DataRowView)DgRooms.SelectedItem;
                int roomId = Convert.ToInt32(rowView["ID"]);

                string query = "DELETE FROM Room WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", roomId);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Номер удален!");
                        LoadRooms();
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
            LoadRoomTypes();
            LoadRooms();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgRooms.SelectedItem is DataRowView rowView)
            {
                TxtRoomNumber.Text = rowView["Номер"].ToString();
                TxtFloor.Text = rowView["Этаж"].ToString();
                TxtPricePerNight.Text = rowView["Цена за ночь"].ToString();

                string roomType = rowView["Тип номера"].ToString();
                foreach (ComboBoxItem item in CmbRoomType.Items)
                {
                    if (item.Content.ToString().Contains(roomType))
                    {
                        CmbRoomType.SelectedItem = item;
                        break;
                    }
                }

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
            TxtRoomNumber.Clear();
            TxtFloor.Clear();
            TxtCapacity.Clear();
            TxtPricePerNight.Clear();
            CmbRoomType.SelectedItem = null;
            CmbStatus.SelectedIndex = 0;
            DgRooms.SelectedItem = null;
        }
    }
}