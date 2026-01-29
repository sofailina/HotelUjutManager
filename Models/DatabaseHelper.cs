using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using HotelUjutManager.Models;

namespace HotelUjutManager
{
    public static class DatabaseHelper
    {
        public static SqlConnection GetConnection()
        {
            string connectionString = "Data Source=S\\STP;Initial Catalog=HotelUjut;Integrated Security=True";
            return new SqlConnection(connectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка подключения к БД: {ex.Message}");
                return false;
            }
        }
    }
}