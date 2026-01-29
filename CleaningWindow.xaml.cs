using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace HotelUjutManager
{
    public partial class CleaningWindow : Window
    {
        private string connectionString = "Data Source=S\\STP;Initial Catalog=HotelUjut;Integrated Security=True";

        public CleaningWindow()
        {
            InitializeComponent();
            Loaded += CleaningWindow_Loaded;
        }

        private void CleaningWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRooms();
            LoadEmployees();
            LoadCleaningData();
        }

        private void LoadRooms()
        {
            try
            {
                CmbRoom.Items.Clear();

                string query = "SELECT id, room_number FROM Room ORDER BY room_number";

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
                                Content = reader["room_number"].ToString(),
                                Tag = reader["id"]
                            };
                            CmbRoom.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки номеров: {ex.Message}");
                CmbRoom.Items.Add(new ComboBoxItem { Content = "101", Tag = 1 });
                CmbRoom.Items.Add(new ComboBoxItem { Content = "102", Tag = 2 });
                CmbRoom.Items.Add(new ComboBoxItem { Content = "201", Tag = 3 });
                CmbRoom.Items.Add(new ComboBoxItem { Content = "304", Tag = 4 });
            }
        }

        private void LoadEmployees()
        {
            try
            {
                CmbEmployee.Items.Clear();

                string query = "SELECT id, full_name FROM Employee ORDER BY full_name";

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
                            CmbEmployee.Items.Add(item);
                        }
                    }
                }
            }
            catch
            {
                CmbEmployee.Items.Add(new ComboBoxItem { Content = "Батурина Ольга Николаевна", Tag = 1 });
                CmbEmployee.Items.Add(new ComboBoxItem { Content = "Иванова Нина Юрьевна", Tag = 2 });
                CmbEmployee.Items.Add(new ComboBoxItem { Content = "Круглова Светлана Леонидовна", Tag = 3 });
            }
        }

        private void LoadCleaningData()
        {
            try
            {
                string query = @"
                    SELECT 
                        c.id,
                        r.room_number,
                        e.full_name as employee_name,
                        c.cleaning_date,
                        c.status
                    FROM Cleaning c
                    JOIN Room r ON c.room_id = r.id
                    JOIN Employee e ON c.employee_id = e.id
                    ORDER BY c.cleaning_date DESC";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var adapter = new SqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table);

                    if (table.Columns.Count > 0)
                    {
                        table.Columns["id"].ColumnName = "ID";
                        table.Columns["room_number"].ColumnName = "Номер комнаты";
                        table.Columns["employee_name"].ColumnName = "Сотрудник";
                        table.Columns["cleaning_date"].ColumnName = "Дата уборки";
                        table.Columns["status"].ColumnName = "Статус";
                    }

                    DgCleaning.ItemsSource = table.DefaultView;

                    MessageBox.Show($"Загружено записей об уборке: {table.Rows.Count}", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка SQL при загрузке данных уборки: {ex.Message}\n" +
                    "Убедитесь, что таблицы Cleaning, Room и Employee существуют.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                var testTable = new DataTable();
                testTable.Columns.Add("ID", typeof(int));
                testTable.Columns.Add("Номер комнаты", typeof(string));
                testTable.Columns.Add("Сотрудник", typeof(string));
                testTable.Columns.Add("Дата уборки", typeof(DateTime));
                testTable.Columns.Add("Статус", typeof(string));

                testTable.Rows.Add(1, "101", "Батурина Ольга Николаевна", DateTime.Now, "Назначено");
                testTable.Rows.Add(2, "102", "Иванова Нина Юрьевна", DateTime.Now, "Выполнено");
                testTable.Rows.Add(3, "201", "Круглова Светлана Леонидовна", DateTime.Now, "Назначено");

                DgCleaning.ItemsSource = testTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            
            if (CmbRoom.SelectedItem == null)
            {
                MessageBox.Show("Выберите номер комнаты!");
                return;
            }

            if (CmbEmployee.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника!");
                return;
            }

            if (!DpCleaningDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите дату уборки!");
                return;
            }

            try
            {
                int roomId = (int)((ComboBoxItem)CmbRoom.SelectedItem).Tag;
                int employeeId = (int)((ComboBoxItem)CmbEmployee.SelectedItem).Tag;
                string status = ((ComboBoxItem)CmbStatus.SelectedItem).Content.ToString();

                string query = @"
                    INSERT INTO Cleaning (room_id, employee_id, cleaning_date, status) 
                    VALUES (@room_id, @employee_id, @cleaning_date, @status)";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@room_id", roomId);
                    command.Parameters.AddWithValue("@employee_id", employeeId);
                    command.Parameters.AddWithValue("@cleaning_date", DpCleaningDate.SelectedDate.Value);
                    command.Parameters.AddWithValue("@status", status);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Уборка успешно назначена!");
                        LoadCleaningData();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления уборки: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (DgCleaning.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись об уборке для редактирования!");
                return;
            }

            if (CmbRoom.SelectedItem == null || CmbEmployee.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            try
            {
                var rowView = (DataRowView)DgCleaning.SelectedItem;
                int cleaningId = Convert.ToInt32(rowView["ID"]);

                int roomId = (int)((ComboBoxItem)CmbRoom.SelectedItem).Tag;
                int employeeId = (int)((ComboBoxItem)CmbEmployee.SelectedItem).Tag;
                string status = ((ComboBoxItem)CmbStatus.SelectedItem).Content.ToString();

                string query = @"
                    UPDATE Cleaning 
                    SET room_id = @room_id, 
                        employee_id = @employee_id, 
                        cleaning_date = @cleaning_date, 
                        status = @status 
                    WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", cleaningId);
                    command.Parameters.AddWithValue("@room_id", roomId);
                    command.Parameters.AddWithValue("@employee_id", employeeId);
                    command.Parameters.AddWithValue("@cleaning_date", DpCleaningDate.SelectedDate.Value);
                    command.Parameters.AddWithValue("@status", status);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Запись об уборке обновлена!");
                        LoadCleaningData();
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
            if (DgCleaning.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранную запись об уборке?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var rowView = (DataRowView)DgCleaning.SelectedItem;
                int cleaningId = Convert.ToInt32(rowView["ID"]);

                string query = "DELETE FROM Cleaning WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", cleaningId);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Запись удалена!");
                        LoadCleaningData();
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
            LoadRooms();
            LoadEmployees();
            LoadCleaningData();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgCleaning_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgCleaning.SelectedItem is DataRowView rowView)
            {

                string roomNumber = rowView["Номер комнаты"].ToString();
                foreach (ComboBoxItem item in CmbRoom.Items)
                {
                    if (item.Content.ToString() == roomNumber)
                    {
                        CmbRoom.SelectedItem = item;
                        break;
                    }
                }

                string employeeName = rowView["Сотрудник"].ToString();
                foreach (ComboBoxItem item in CmbEmployee.Items)
                {
                    if (item.Content.ToString() == employeeName)
                    {
                        CmbEmployee.SelectedItem = item;
                        break;
                    }
                }

                if (rowView["Дата уборки"] != DBNull.Value)
                    DpCleaningDate.SelectedDate = Convert.ToDateTime(rowView["Дата уборки"]);

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
            CmbRoom.SelectedItem = null;
            CmbEmployee.SelectedItem = null;
            DpCleaningDate.SelectedDate = DateTime.Now;
            CmbStatus.SelectedIndex = 0;
            DgCleaning.SelectedItem = null;
        }
    }
}