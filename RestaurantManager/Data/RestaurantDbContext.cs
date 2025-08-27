using Microsoft.EntityFrameworkCore;
using RestaurantManager.Models;
using System;

namespace RestaurantManager.Data
{
    public class RestaurantDbContext : DbContext
    {
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<Table> Tables => Set<Table>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<User> Users => Set<User>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "restaurant.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relații
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Table)
                .WithMany()
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany()
                .HasForeignKey(oi => oi.MenuItemId);

            // Proprietate doar pentru UI, să nu fie mapată în DB
            modelBuilder.Entity<OrderItem>().Ignore(oi => oi.Subtotal);

            // Seed meniu
            modelBuilder.Entity<MenuItem>().HasData(
                new MenuItem { Id = 1, Name = "Ciorbă de burtă", Category = "Supe", Price = 22.50m, IsAvailable = true },
                new MenuItem { Id = 2, Name = "Mici", Category = "Grătar", Price = 6.00m, IsAvailable = true },
                new MenuItem { Id = 3, Name = "Papanași", Category = "Desert", Price = 18.00m, IsAvailable = true }
            );

            // Seed mese
            modelBuilder.Entity<Table>().HasData(
                new Table { Id = 1, Name = "Masa 1", IsActive = true },
                new Table { Id = 2, Name = "Masa 2", IsActive = true },
                new Table { Id = 3, Name = "Masa 3", IsActive = true }
            );

            // Seed utilizatori DEMO (parole în clar doar pentru prototip)
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "admin", Role = "Admin", IsActive = true },
                new User { Id = 2, Username = "ospatar", Password = "1234", Role = "Waiter", IsActive = true }
            );
        }
    }
}
