using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace HotelUjutManager
{
    public partial class PaymentsWindow : Window
    {
        private string connectionString = "Data Source=S\\STP;Initial Catalog=HotelUjut;Integrated Security=True";

        public PaymentsWindow()
        {
            InitializeComponent();
            Loaded += PaymentsWindow_Loaded;
        }

        private void PaymentsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBookings();
            LoadPayments();
        }

        private void LoadBookings()
        {
            try
            {
                CmbBooking.Items.Clear();

                string query = @"
                    SELECT b.id, g.full_name, r.room_number
                    FROM Booking b
                    JOIN Guest g ON b.guest_id = g.id
                    JOIN Room r ON b.room_id = r.id
                    ORDER BY b.id DESC";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string bookingInfo = $"#{reader["id"]} - {reader["full_name"]} (комната {reader["room_number"]})";
                            var item = new ComboBoxItem
                            {
                                Content = bookingInfo,
                                Tag = reader["id"]
                            };
                            CmbBooking.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки бронирований: {ex.Message}");
                CmbBooking.Items.Add(new ComboBoxItem { Content = "#101 - Петренко В.Л. (комната 101)", Tag = 1 });
                CmbBooking.Items.Add(new ComboBoxItem { Content = "#102 - Смирнова Е.И. (комната 102)", Tag = 2 });
            }
        }

        private void LoadPayments()
        {
            try
            {
                string query = @"
                    SELECT 
                        p.id,
                        p.booking_id,
                        g.full_name as guest_name,
                        p.amount,
                        p.payment_date,
                        p.status
                    FROM Payment p
                    JOIN Booking b ON p.booking_id = b.id
                    JOIN Guest g ON b.guest_id = g.id
                    ORDER BY p.payment_date DESC";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var adapter = new SqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table);

                    if (table.Columns.Count > 0)
                    {
                        table.Columns["id"].ColumnName = "ID";
                        table.Columns["booking_id"].ColumnName = "ID бронирования";
                        table.Columns["guest_name"].ColumnName = "Гость";
                        table.Columns["amount"].ColumnName = "Сумма";
                        table.Columns["payment_date"].ColumnName = "Дата платежа";
                        table.Columns["status"].ColumnName = "Статус";
                    }

                    DgPayments.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки платежей: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CmbBooking.SelectedItem == null)
            {
                MessageBox.Show("Выберите бронирование!");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtAmount.Text) || !decimal.TryParse(TxtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Введите корректную сумму!");
                return;
            }

            try
            {
                int bookingId = (int)((ComboBoxItem)CmbBooking.SelectedItem).Tag;
                string status = ((ComboBoxItem)CmbStatus.SelectedItem).Content.ToString();

                string query = "INSERT INTO Payment (booking_id, amount, payment_date, status) VALUES (@booking_id, @amount, @payment_date, @status)";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@booking_id", bookingId);
                    command.Parameters.AddWithValue("@amount", amount);
                    command.Parameters.AddWithValue("@payment_date", DpPaymentDate.SelectedDate ?? DateTime.Now);
                    command.Parameters.AddWithValue("@status", status);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Платеж успешно добавлен!");
                        LoadPayments();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления платежа: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (DgPayments.SelectedItem == null)
            {
                MessageBox.Show("Выберите платеж для редактирования!");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtAmount.Text) || !decimal.TryParse(TxtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Введите корректную сумму!");
                return;
            }

            try
            {
                var rowView = (DataRowView)DgPayments.SelectedItem;
                int paymentId = Convert.ToInt32(rowView["ID"]);

                int bookingId = (int)((ComboBoxItem)CmbBooking.SelectedItem).Tag;
                string status = ((ComboBoxItem)CmbStatus.SelectedItem).Content.ToString();

                string query = "UPDATE Payment SET booking_id = @booking_id, amount = @amount, payment_date = @payment_date, status = @status WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", paymentId);
                    command.Parameters.AddWithValue("@booking_id", bookingId);
                    command.Parameters.AddWithValue("@amount", amount);
                    command.Parameters.AddWithValue("@payment_date", DpPaymentDate.SelectedDate ?? DateTime.Now);
                    command.Parameters.AddWithValue("@status", status);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Платеж обновлен!");
                        LoadPayments();
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
            if (DgPayments.SelectedItem == null)
            {
                MessageBox.Show("Выберите платеж для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранный платеж?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var rowView = (DataRowView)DgPayments.SelectedItem;
                int paymentId = Convert.ToInt32(rowView["ID"]);

                string query = "DELETE FROM Payment WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", paymentId);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Платеж удален!");
                        LoadPayments();
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
            LoadBookings();
            LoadPayments();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgPayments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgPayments.SelectedItem is DataRowView rowView)
            {
                int bookingId = Convert.ToInt32(rowView["ID бронирования"]);
                foreach (ComboBoxItem item in CmbBooking.Items)
                {
                    if ((int)item.Tag == bookingId)
                    {
                        CmbBooking.SelectedItem = item;
                        break;
                    }
                }

                TxtAmount.Text = rowView["Сумма"].ToString();

                if (rowView["Дата платежа"] != DBNull.Value)
                    DpPaymentDate.SelectedDate = Convert.ToDateTime(rowView["Дата платежа"]);

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
            CmbBooking.SelectedItem = null;
            TxtAmount.Clear();
            DpPaymentDate.SelectedDate = DateTime.Now;
            CmbStatus.SelectedIndex = 0;
            DgPayments.SelectedItem = null;
        }
    }
}