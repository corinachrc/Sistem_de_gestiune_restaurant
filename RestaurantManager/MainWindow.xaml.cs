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

            // Setăm DataContext separat pe fiecare tab
            MenuTab.DataContext = new MenuItemsViewModel();
            OrdersTab.DataContext = new OrdersViewModel();

            // Dacă e OSPĂTAR: ascundem "Meniu" și selectăm explicit "Comenzi"
            if (Session.IsWaiter)
            {
                MenuTab.Visibility = Visibility.Collapsed;
                OrdersTab.IsSelected = true;       // << important
            }

            // Titlu cu userul curent (opțional)
            if (Session.CurrentUser != null)
                Title += $" — {Session.CurrentUser.Username} ({Session.CurrentUser.Role})";
        }
    }
}
