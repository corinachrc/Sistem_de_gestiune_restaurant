using System.Windows;
using Microsoft.EntityFrameworkCore;
using RestaurantManager.Data;

namespace RestaurantManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Aplică migrațiile (creează tabelele dacă lipsesc)
            using (var db = new RestaurantDbContext())
                db.Database.Migrate();

            // IMPORTANT: nu lăsăm aplicația să se închidă când se închide Login
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Login modal
            var login = new LoginWindow();
            var ok = login.ShowDialog() == true;

            if (!ok)
            {
                Shutdown(); // a apăsat Anulează sau a închis Login
                return;
            }

            // Deschidem fereastra principală și o setăm ca MainWindow
            var main = new MainWindow();
            MainWindow = main;

            // Revenim la comportamentul normal (aplicația se închide când se închide MainWindow)
            ShutdownMode = ShutdownMode.OnLastWindowClose;

            main.Show();
        }
    }
}
