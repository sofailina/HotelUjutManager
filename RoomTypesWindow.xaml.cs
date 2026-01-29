using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace HotelUjutManager
{
    public partial class RoomTypesWindow : Window
    {
        private string connectionString = "Data Source=S\\STP;Initial Catalog=HotelUjut;Integrated Security=True";

        public RoomTypesWindow()
        {
            InitializeComponent();
            Loaded += RoomTypesWindow_Loaded;
        }

        private void RoomTypesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRoomTypes();
        }

        private void LoadRoomTypes()
        {
            try
            {
                string query = "SELECT id, name, description, price_per_night FROM RoomType ORDER BY name";

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
                        table.Columns["price_per_night"].ColumnName = "Цена за ночь";
                    }

                    DgRoomTypes.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов номеров: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Введите название типа номера!");
                return;
            }

            if (!decimal.TryParse(TxtPricePerNight.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену!");
                return;
            }

            try
            {
                string query = "INSERT INTO RoomType (name, description, price_per_night) VALUES (@name, @description, @price_per_night)";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", TxtName.Text);
                    command.Parameters.AddWithValue("@description", TxtDescription.Text ?? "");
                    command.Parameters.AddWithValue("@price_per_night", price);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Тип номера успешно добавлен!");
                        LoadRoomTypes();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления типа номера: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (DgRoomTypes.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип номера для редактирования!");
                return;
            }

            if (!decimal.TryParse(TxtPricePerNight.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену!");
                return;
            }

            try
            {
                var rowView = (DataRowView)DgRoomTypes.SelectedItem;
                int roomTypeId = Convert.ToInt32(rowView["ID"]);

                string query = "UPDATE RoomType SET name = @name, description = @description, price_per_night = @price_per_night WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", roomTypeId);
                    command.Parameters.AddWithValue("@name", TxtName.Text);
                    command.Parameters.AddWithValue("@description", TxtDescription.Text ?? "");
                    command.Parameters.AddWithValue("@price_per_night", price);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Тип номера обновлен!");
                        LoadRoomTypes();
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
            if (DgRoomTypes.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип номера для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранный тип номера?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var rowView = (DataRowView)DgRoomTypes.SelectedItem;
                int roomTypeId = Convert.ToInt32(rowView["ID"]);

                string query = "DELETE FROM RoomType WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", roomTypeId);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Тип номера удален!");
                        LoadRoomTypes();
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
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgRoomTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgRoomTypes.SelectedItem is DataRowView rowView)
            {
                TxtName.Text = rowView["Название"].ToString();
                TxtDescription.Text = rowView["Описание"].ToString();
                TxtPricePerNight.Text = rowView["Цена за ночь"].ToString();
            }
        }

        private void ClearForm()
        {
            TxtName.Clear();
            TxtDescription.Clear();
            TxtPricePerNight.Clear();
            TxtCapacity.Clear();
            DgRoomTypes.SelectedItem = null;
        }
    }
}