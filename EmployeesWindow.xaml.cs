using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace HotelUjutManager
{
    public partial class EmployeesWindow : Window
    {
        private string connectionString = "Data Source=S\\STP;Initial Catalog=HotelUjut;Integrated Security=True";

        public EmployeesWindow()
        {
            InitializeComponent();
            Loaded += EmployeesWindow_Loaded;
        }

        private void EmployeesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                string query = "SELECT id, full_name, position FROM Employee ORDER BY full_name";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var adapter = new SqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table);

                    if (table.Columns.Count > 0)
                    {
                        table.Columns["id"].ColumnName = "ID";
                        table.Columns["full_name"].ColumnName = "ФИО";
                        table.Columns["position"].ColumnName = "Должность";
                    }

                    DgEmployees.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFullName.Text))
            {
                MessageBox.Show("Введите ФИО сотрудника!");
                return;
            }

            try
            {
                string query = "INSERT INTO Employee (full_name, position) VALUES (@full_name, @position)";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@full_name", TxtFullName.Text);
                    command.Parameters.AddWithValue("@position", TxtPosition.Text ?? "");

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Сотрудник успешно добавлен!");
                        LoadEmployees();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления сотрудника: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (DgEmployees.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника для редактирования!");
                return;
            }

            try
            {
                var rowView = (DataRowView)DgEmployees.SelectedItem;
                int employeeId = Convert.ToInt32(rowView["ID"]);

                string query = "UPDATE Employee SET full_name = @full_name, position = @position WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", employeeId);
                    command.Parameters.AddWithValue("@full_name", TxtFullName.Text);
                    command.Parameters.AddWithValue("@position", TxtPosition.Text ?? "");

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Изменения сохранены!");
                        LoadEmployees();
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
            if (DgEmployees.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранного сотрудника?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var rowView = (DataRowView)DgEmployees.SelectedItem;
                int employeeId = Convert.ToInt32(rowView["ID"]);

                string query = "DELETE FROM Employee WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", employeeId);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Сотрудник удален!");
                        LoadEmployees();
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
            LoadEmployees();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgEmployees.SelectedItem is DataRowView rowView)
            {
                TxtFullName.Text = rowView["ФИО"].ToString();
                TxtPosition.Text = rowView["Должность"].ToString();
            }
        }
        private void ClearForm()
        {
            TxtFullName.Clear();
            TxtPosition.Clear();
            DgEmployees.SelectedItem = null;
        }
    }
}