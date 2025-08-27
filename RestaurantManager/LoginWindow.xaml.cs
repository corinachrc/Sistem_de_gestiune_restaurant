using System.Linq;
using System.Windows;
using RestaurantManager.Auth;
using RestaurantManager.Data;

namespace RestaurantManager
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();         // <- trebuie să existe după fix
            UsernameBox.Text = string.Empty;
            PasswordBox.Clear();
            UsernameBox.Focus();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var user = UsernameBox.Text.Trim();
            var pass = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Completează utilizator și parolă.");
                return;
            }

            try
            {
                using var db = new RestaurantDbContext();
                var found = db.Users.FirstOrDefault(u => u.Username == user && u.Password == pass && u.IsActive);
                if (found == null)
                {
                    MessageBox.Show("Utilizator sau parolă greșite.");
                    return;
                }

                Session.CurrentUser = found;
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Eroare la autentificare: " + ex.Message, "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
