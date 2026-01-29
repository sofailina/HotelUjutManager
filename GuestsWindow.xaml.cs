using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace HotelUjutManager
{
    public partial class GuestsWindow : Window
    {
        private string connectionString = "Data Source=S\\STP;Initial Catalog=HotelUjut;Integrated Security=True";

        public GuestsWindow()
        {
            InitializeComponent();
            Loaded += GuestsWindow_Loaded;
        }

        private void GuestsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGuests();
        }

        private void LoadGuests()
        {
            try
            {
               
                string query = "SELECT id, full_name, passport, phone, email, birth_date, address FROM Guest";

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
                        table.Columns["passport"].ColumnName = "Паспорт";
                        table.Columns["phone"].ColumnName = "Телефон";
                        table.Columns["email"].ColumnName = "Email";
                        table.Columns["birth_date"].ColumnName = "Дата рождения";
                        table.Columns["address"].ColumnName = "Адрес";
                    }

                    DgGuests.ItemsSource = table.DefaultView;

                   
                    MessageBox.Show($"Загружено записей: {table.Rows.Count}", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка SQL: {ex.Message}\nПроверьте подключение к базе данных.",
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
            if (string.IsNullOrWhiteSpace(TxtFullName.Text))
            {
                MessageBox.Show("Введите ФИО гостя!");
                return;
            }

            try
            {
                string query = @"
                    INSERT INTO Guest (full_name, passport, phone, email, birth_date, address) 
                    VALUES (@full_name, @passport, @phone, @email, @birth_date, @address)";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@full_name", TxtFullName.Text);
                    command.Parameters.AddWithValue("@passport", TxtPassport.Text ?? "");
                    command.Parameters.AddWithValue("@phone", TxtPhone.Text ?? "");
                    command.Parameters.AddWithValue("@email", TxtEmail.Text ?? "");

                    if (DpBirthDate.SelectedDate.HasValue)
                        command.Parameters.AddWithValue("@birth_date", DpBirthDate.SelectedDate.Value);
                    else
                        command.Parameters.AddWithValue("@birth_date", DBNull.Value);

                    command.Parameters.AddWithValue("@address", TxtAddress.Text ?? "");

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Гость успешно добавлен!");
                        LoadGuests();
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления гостя: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (DgGuests.SelectedItem == null)
            {
                MessageBox.Show("Выберите гостя для редактирования!");
                return;
            }

            try
            {
                var rowView = (DataRowView)DgGuests.SelectedItem;
                int guestId = Convert.ToInt32(rowView["ID"]);

                string query = @"
                    UPDATE Guest 
                    SET full_name = @full_name, 
                        passport = @passport, 
                        phone = @phone, 
                        email = @email, 
                        birth_date = @birth_date, 
                        address = @address 
                    WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", guestId);
                    command.Parameters.AddWithValue("@full_name", TxtFullName.Text);
                    command.Parameters.AddWithValue("@passport", TxtPassport.Text ?? "");
                    command.Parameters.AddWithValue("@phone", TxtPhone.Text ?? "");
                    command.Parameters.AddWithValue("@email", TxtEmail.Text ?? "");

                    if (DpBirthDate.SelectedDate.HasValue)
                        command.Parameters.AddWithValue("@birth_date", DpBirthDate.SelectedDate.Value);
                    else
                        command.Parameters.AddWithValue("@birth_date", DBNull.Value);

                    command.Parameters.AddWithValue("@address", TxtAddress.Text ?? "");

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Изменения сохранены!");
                        LoadGuests();
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
            if (DgGuests.SelectedItem == null)
            {
                MessageBox.Show("Выберите гостя для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранного гостя?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var rowView = (DataRowView)DgGuests.SelectedItem;
                int guestId = Convert.ToInt32(rowView["ID"]);

                string query = "DELETE FROM Guest WHERE id = @id";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", guestId);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Гость удален!");
                        LoadGuests();
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
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgGuests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgGuests.SelectedItem is DataRowView rowView)
            {
                TxtFullName.Text = rowView["ФИО"].ToString();
                TxtPassport.Text = rowView["Паспорт"].ToString();
                TxtPhone.Text = rowView["Телефон"].ToString();
                TxtEmail.Text = rowView["Email"].ToString();

                object birthDateValue = rowView["Дата рождения"];
                if (birthDateValue != DBNull.Value && birthDateValue != null && !string.IsNullOrEmpty(birthDateValue.ToString()))
                {
                    if (DateTime.TryParse(birthDateValue.ToString(), out DateTime birthDate))
                    {
                        DpBirthDate.SelectedDate = birthDate;
                    }
                }
                else
                {
                    DpBirthDate.SelectedDate = null;
                }

                TxtAddress.Text = rowView["Адрес"].ToString();
            }
        }

        private void ClearForm()
        {
            TxtFullName.Clear();
            TxtPassport.Clear();
            TxtPhone.Clear();
            TxtEmail.Clear();
            DpBirthDate.SelectedDate = null;
            TxtAddress.Clear();
            DgGuests.SelectedItem = null;
        }
    }
}