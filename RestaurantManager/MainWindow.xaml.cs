using System.Windows;
using RestaurantManager.ViewModels;
using RestaurantManager.Auth;

namespace RestaurantManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // DataContext per tab
            MenuTab.DataContext = new MenuItemsViewModel();
            OrdersTab.DataContext = new OrdersViewModel();
            UsersTab.DataContext = new UsersViewModel();

            ApplyRoleVisibility();

            if (Session.CurrentUser != null)
                Title += $" — {Session.CurrentUser.Username} ({Session.CurrentUser.Role})";
        }

        private void ApplyRoleVisibility()
        {
            if (Session.IsWaiter)
            {
                MenuTab.Visibility = Visibility.Collapsed;
                UsersTab.Visibility = Visibility.Collapsed;
                OrdersTab.IsSelected = true;
            }
            else // Admin
            {
                MenuTab.Visibility = Visibility.Visible;
                UsersTab.Visibility = Visibility.Visible;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.Hide();

            Session.CurrentUser = null;
            var ok = new LoginWindow().ShowDialog() == true;

            this.Close();

            if (!ok)
            {
                Application.Current.Shutdown();
                return;
            }

            var newMain = new MainWindow();
            Application.Current.MainWindow = newMain;
            Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            newMain.Show();
        }
    }
}
