using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace HotelUjutManager
{
    public partial class ServicesWindow : Window
    {
        private string connectionString = "Data Source=S\\STP;Initial Catalog=HotelUjut;Integrated Security=True";

        public ServicesWindow()
        {
            InitializeComponent();
            Loaded += ServicesWindow_Loaded;
        }

        private void ServicesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServices();
        }

        private void LoadServices()
        {
            try
            {
                string query = "SELECT id, name, description, price FROM Service ORDER BY name";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var adapter = new SqlDataAdapter(query, connection);
                    var table = new DataTable();
                    adapter.Fill(table);

                    if (table.Columns.Count > 0)
                    {
                        table.Columns["id"].ColumnName = "ID";
                        table.Columns["name"].ColumnName = "Название";
                        table.Columns["description"].ColumnName = "Описание";
                        table.Columns["price"].ColumnName = "Цена";
                    }

                    DgServices.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Введите название услуги!");
                return;
            }

            if (!decimal.TryParse(TxtPrice.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену!");
                return;
            }

            try
            {
                string query = "INSERT INTO Service (name, description, price) VALUES (@name, @description, @price)";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", TxtName.Text);
                    command.Parameters.AddWithValue("@description", TxtDescription.Text ?? "");
                    command.Parameters.AddWithValue("@price", price);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Услуга успешно добавлена!");
                        LoadServices();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления услуги: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (DgServices.SelectedItem == null)
            {
                MessageBox.Show("Выберите услугу для редактирования!");
                return;
            }

            if (!decimal.TryParse(TxtPrice.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену!");
                return;
            }

            try
            {
                var rowView = (DataRowView)DgServices.SelectedItem;
                int serviceId = Convert.ToInt32(rowView["ID"]);

                string query = "UPDATE Service SET name = @name, description = @description, price = @price WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", serviceId);
                    command.Parameters.AddWithValue("@name", TxtName.Text);
                    command.Parameters.AddWithValue("@description", TxtDescription.Text ?? "");
                    command.Parameters.AddWithValue("@price", price);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Услуга обновлена!");
                        LoadServices();
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
            if (DgServices.SelectedItem == null)
            {
                MessageBox.Show("Выберите услугу для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранную услугу?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var rowView = (DataRowView)DgServices.SelectedItem;
                int serviceId = Convert.ToInt32(rowView["ID"]);

                string query = "DELETE FROM Service WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", serviceId);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Услуга удалена!");
                        LoadServices();
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
            LoadServices();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgServices.SelectedItem is DataRowView rowView)
            {
                TxtName.Text = rowView["Название"].ToString();
                TxtDescription.Text = rowView["Описание"].ToString();
                TxtPrice.Text = rowView["Цена"].ToString();
            }
        }

        private void ClearForm()
        {
            TxtName.Clear();
            TxtDescription.Clear();
            TxtPrice.Clear();
            DgServices.SelectedItem = null;
        }
    }
}