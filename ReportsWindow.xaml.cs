using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;

namespace HotelUjutManager
{
    public partial class ReportsWindow : Window
    {
        private string connectionString = "Data Source=S\\STP;Initial Catalog=HotelUjut;Integrated Security=True";

        public ReportsWindow()
        {
            InitializeComponent();
            Loaded += ReportsWindow_Loaded;
        }

        private void ReportsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DpPeriodStart.SelectedDate = DateTime.Now.AddDays(-30);
            DpPeriodEnd.SelectedDate = DateTime.Now;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (!DpPeriodStart.SelectedDate.HasValue || !DpPeriodEnd.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите период!");
                return;
            }

            string reportType = ((ComboBoxItem)CmbReportType.SelectedItem).Content.ToString();
            DateTime startDate = DpPeriodStart.SelectedDate.Value;
            DateTime endDate = DpPeriodEnd.SelectedDate.Value;

            try
            {
                DataTable reportData = new DataTable();

                switch (reportType)
                {
                    case "Загрузка номеров":
                        reportData = GenerateRoomOccupancyReport(startDate, endDate);
                        break;
                    case "Финансовый отчет":
                        reportData = GenerateFinancialReport(startDate, endDate);
                        break;
                    case "Отчет по уборке":
                        reportData = GenerateCleaningReport(startDate, endDate);
                        break;
                    case "Отчет по бронированиям":
                        reportData = GenerateBookingReport(startDate, endDate);
                        break;
                }

                DgReport.ItemsSource = reportData.DefaultView;
                SaveReportToDatabase(reportType, startDate, endDate, reportData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации отчета: {ex.Message}");
            }
        }

        private DataTable GenerateRoomOccupancyReport(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT 
                    r.room_number as 'Номер',
                    COUNT(b.id) as 'Кол-во бронирований',
                    SUM(DATEDIFF(day, 
                        CASE WHEN b.check_in > @startDate THEN b.check_in ELSE @startDate END,
                        CASE WHEN b.check_out < @endDate THEN b.check_out ELSE @endDate END
                    )) as 'Занято дней',
                    CONCAT(CAST(CAST(SUM(DATEDIFF(day, 
                        CASE WHEN b.check_in > @startDate THEN b.check_in ELSE @startDate END,
                        CASE WHEN b.check_out < @endDate THEN b.check_out ELSE @endDate END
                    )) as float) / DATEDIFF(day, @startDate, @endDate) * 100 as DECIMAL(5,2)), '%') as 'Загрузка'
                FROM Room r
                LEFT JOIN Booking b ON r.id = b.room_id 
                    AND b.check_in <= @endDate 
                    AND b.check_out >= @startDate
                    AND b.status != 'Отменено'
                GROUP BY r.room_number
                ORDER BY r.room_number";

            return ExecuteReportQuery(query, startDate, endDate);
        }

        private DataTable GenerateFinancialReport(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT 
                    FORMAT(p.payment_date, 'yyyy-MM') as 'Месяц',
                    COUNT(p.id) as 'Кол-во платежей',
                    SUM(p.amount) as 'Сумма',
                    AVG(p.amount) as 'Средний чек'
                FROM Payment p
                WHERE p.payment_date BETWEEN @startDate AND @endDate
                    AND p.status = 'Оплачено'
                GROUP BY FORMAT(p.payment_date, 'yyyy-MM')
                ORDER BY FORMAT(p.payment_date, 'yyyy-MM') DESC";

            return ExecuteReportQuery(query, startDate, endDate);
        }

        private DataTable GenerateCleaningReport(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT 
                    e.full_name as 'Сотрудник',
                    COUNT(c.id) as 'Кол-во уборок',
                    SUM(CASE WHEN c.status = 'Выполнено' THEN 1 ELSE 0 END) as 'Выполнено',
                    SUM(CASE WHEN c.status = 'Назначено' THEN 1 ELSE 0 END) as 'Назначено',
                    SUM(CASE WHEN c.status = 'Отменено' THEN 1 ELSE 0 END) as 'Отменено'
                FROM Cleaning c
                JOIN Employee e ON c.employee_id = e.id
                WHERE c.cleaning_date BETWEEN @startDate AND @endDate
                GROUP BY e.full_name
                ORDER BY COUNT(c.id) DESC";

            return ExecuteReportQuery(query, startDate, endDate);
        }

        private DataTable GenerateBookingReport(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT 
                    g.full_name as 'Гость',
                    COUNT(b.id) as 'Кол-во бронирований',
                    SUM(DATEDIFF(day, b.check_in, b.check_out)) as 'Всего ночей',
                    SUM(CASE WHEN b.status = 'Подтверждено' THEN 1 ELSE 0 END) as 'Подтверждено',
                    SUM(CASE WHEN b.status = 'Отменено' THEN 1 ELSE 0 END) as 'Отменено'
                FROM Booking b
                JOIN Guest g ON b.guest_id = g.id
                WHERE b.check_in BETWEEN @startDate AND @endDate
                GROUP BY g.full_name
                ORDER BY COUNT(b.id) DESC";

            return ExecuteReportQuery(query, startDate, endDate);
        }

        private DataTable ExecuteReportQuery(string query, DateTime startDate, DateTime endDate)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);

                connection.Open();
                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private void SaveReportToDatabase(string reportType, DateTime periodStart, DateTime periodEnd, DataTable data)
        {
            try
            {
                string dataJson = DataTableToJson(data);

                string query = @"
                    INSERT INTO Report (report_type, period_start, period_end, data) 
                    VALUES (@report_type, @period_start, @period_end, @data)";

                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@report_type", reportType);
                    command.Parameters.AddWithValue("@period_start", periodStart);
                    command.Parameters.AddWithValue("@period_end", periodEnd);
                    command.Parameters.AddWithValue("@data", dataJson);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения отчета в БД: {ex.Message}");
            }
        }

        private string DataTableToJson(DataTable table)
        {
            var jsonString = new System.Text.StringBuilder();
            jsonString.Append("[");

            for (int i = 0; i < table.Rows.Count; i++)
            {
                jsonString.Append("{");
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    jsonString.Append($"\"{table.Columns[j].ColumnName}\":\"{table.Rows[i][j]}\"");
                    if (j < table.Columns.Count - 1) jsonString.Append(",");
                }
                jsonString.Append("}");
                if (i < table.Rows.Count - 1) jsonString.Append(",");
            }

            jsonString.Append("]");
            return jsonString.ToString();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (DgReport.ItemsSource == null)
            {
                MessageBox.Show("Сначала сформируйте отчет!");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV файл (*.csv)|*.csv|Текстовый файл (*.txt)|*.txt";
            saveDialog.FileName = $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (saveDialog.ShowDialog() == true)
            {
                ExportToFile(saveDialog.FileName);
            }
        }

        private void ExportToFile(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    DataView dataView = (DataView)DgReport.ItemsSource;
                    DataTable table = dataView.Table;

                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        writer.Write(table.Columns[i].ColumnName);
                        writer.Write(i == table.Columns.Count - 1 ? "\n" : ";");
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            writer.Write(row[i].ToString());
                            writer.Write(i == table.Columns.Count - 1 ? "\n" : ";");
                        }
                    }
                }

                MessageBox.Show($"Отчет сохранен: {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}");
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            DgReport.ItemsSource = null;
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            BtnSave_Click(sender, e);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция печати будет реализована позже", "Информация");
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}