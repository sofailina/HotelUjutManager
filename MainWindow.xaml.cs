using System.Windows;

namespace HotelUjutManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DatabaseHelper.TestConnection())
            {
                StatusText.Text = "БД подключена";
            }
            else
            {
                StatusText.Text = "Ошибка подключения к БД";
            }
        }

        private void BookingButton_Click(object sender, RoutedEventArgs e)
        {
            var bookingWindow = new BookingWindow();
            bookingWindow.ShowDialog();
            StatusText.Text = "Окно бронирования закрыто";
        }

        private void GuestsButton_Click(object sender, RoutedEventArgs e)
        {
            var guestsWindow = new GuestsWindow();
            guestsWindow.ShowDialog();
            StatusText.Text = "Окно гостей закрыто";
        }

        private void RoomsButton_Click(object sender, RoutedEventArgs e)
        {
            var roomsWindow = new RoomsWindow();
            roomsWindow.ShowDialog();
            StatusText.Text = "Окно номеров закрыто";
        }

        private void CleaningButton_Click(object sender, RoutedEventArgs e)
        {
            var cleaningWindow = new CleaningWindow();
            cleaningWindow.ShowDialog();
            StatusText.Text = "Окно уборки закрыто";
        }

        private void PaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            var paymentsWindow = new PaymentsWindow();
            paymentsWindow.ShowDialog(); 
            StatusText.Text = "Окно оплат закрыто";
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new ReportsWindow();
            reportsWindow.ShowDialog();
            StatusText.Text = "Окно отчетов закрыто";
        }

        private void ServicesButton_Click(object sender, RoutedEventArgs e)
        {
            var servicesWindow = new ServicesWindow();
            servicesWindow.ShowDialog();
            StatusText.Text = "Окно услуг закрыто";
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            var employeesWindow = new EmployeesWindow();
            employeesWindow.ShowDialog();
            StatusText.Text = "Окно сотрудников закрыто";
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}