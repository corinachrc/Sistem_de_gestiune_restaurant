using System.Collections.ObjectModel;
using System.Linq;
using RestaurantManager.Data;
using RestaurantManager.MVVM;
using RestaurantManager.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace RestaurantManager.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        public ObservableCollection<Table> Tables { get; } = new();
        public ObservableCollection<MenuItem> Menu { get; } = new();
        public ObservableCollection<OrderItem> CurrentItems { get; } = new();

        private Table? _selectedTable;
        private Order? _currentOrder;

        public Table? SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (SetProperty(ref _selectedTable, value))
                {
                    LoadOrCreateOpenOrder();
                    UpdateTotals();
                    PayCommand.RaiseCanExecuteChanged();
                    AddItemCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private decimal _total;
        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        private decimal _revenueToday;
        public decimal RevenueToday
        {
            get => _revenueToday;
            set => SetProperty(ref _revenueToday, value);
        }

        private decimal _revenueAllTime;
        public decimal RevenueAllTime
        {
            get => _revenueAllTime;
            set => SetProperty(ref _revenueAllTime, value);
        }

        public RelayCommand AddItemCommand { get; }
        public RelayCommand IncreaseQtyCommand { get; }
        public RelayCommand DecreaseQtyCommand { get; }
        public RelayCommand RemoveItemCommand { get; }
        public RelayCommand PayCommand { get; }
        public RelayCommand RefreshRevenueCommand { get; }

        private OrderItem? _selectedOrderItem;
        public OrderItem? SelectedOrderItem
        {
            get => _selectedOrderItem;
            set
            {
                if (SetProperty(ref _selectedOrderItem, value))
                {
                    IncreaseQtyCommand.RaiseCanExecuteChanged();
                    DecreaseQtyCommand.RaiseCanExecuteChanged();
                    RemoveItemCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public OrdersViewModel()
        {
            AddItemCommand = new RelayCommand(AddMenuItemToOrder, _ => _currentOrder != null);
            IncreaseQtyCommand = new RelayCommand(_ => ChangeQty(+1), _ => SelectedOrderItem != null);
            DecreaseQtyCommand = new RelayCommand(_ => ChangeQty(-1), _ => SelectedOrderItem != null && SelectedOrderItem.Quantity > 1);
            RemoveItemCommand = new RelayCommand(_ => RemoveSelectedItem(), _ => SelectedOrderItem != null);
            PayCommand = new RelayCommand(_ => Pay(), _ => _currentOrder != null && CurrentItems.Count > 0);
            RefreshRevenueCommand = new RelayCommand(_ => UpdateRevenues());

            LoadLookups();
            UpdateRevenues();
        }

        private void LoadLookups()
        {
            Tables.Clear(); Menu.Clear();
            using var db = new RestaurantDbContext();
            foreach (var t in db.Tables.Where(t => t.IsActive).OrderBy(t => t.Id)) Tables.Add(t);
            foreach (var m in db.MenuItems.Where(m => m.IsAvailable).OrderBy(m => m.Category).ThenBy(m => m.Name)) Menu.Add(m);
        }

        private void LoadOrCreateOpenOrder()
        {
            CurrentItems.Clear();
            _currentOrder = null;
            if (SelectedTable == null) return;

            using var db = new RestaurantDbContext();

            _currentOrder = db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
                .FirstOrDefault(o => o.TableId == SelectedTable.Id && o.Status == "Open");

            if (_currentOrder == null)
            {
                _currentOrder = new Order { TableId = SelectedTable.Id, Status = "Open", CreatedAt = DateTime.Now };
                db.Orders.Add(_currentOrder);
                db.SaveChanges();
            }

            foreach (var i in _currentOrder.Items.OrderBy(i => i.Id))
                CurrentItems.Add(i);

            PayCommand.RaiseCanExecuteChanged();
            AddItemCommand.RaiseCanExecuteChanged();
        }

        private void AddMenuItemToOrder(object? parameter)
        {
            if (_currentOrder == null || parameter is not MenuItem menuItem) return;

            using var db = new RestaurantDbContext();
            var order = db.Orders.Include(o => o.Items).First(o => o.Id == _currentOrder.Id);

            var existing = order.Items.FirstOrDefault(i => i.MenuItemId == menuItem.Id);
            if (existing == null)
            {
                var newItem = new OrderItem
                {
                    OrderId = order.Id,
                    MenuItemId = menuItem.Id,
                    Quantity = 1,
                    UnitPrice = menuItem.Price
                };
                db.OrderItems.Add(newItem);
                db.SaveChanges();

                var loaded = db.OrderItems.Include(i => i.MenuItem).First(i => i.Id == newItem.Id);
                CurrentItems.Add(loaded);
            }
            else
            {
                existing.Quantity += 1;
                db.SaveChanges();

                var uiItem = CurrentItems.First(i => i.Id == existing.Id);
                uiItem.Quantity = existing.Quantity;
            }

            UpdateTotals();
            DecreaseQtyCommand.RaiseCanExecuteChanged();
            RemoveItemCommand.RaiseCanExecuteChanged();
            PayCommand.RaiseCanExecuteChanged();
        }

        private void ChangeQty(int delta)
        {
            if (SelectedOrderItem == null) return;

            using var db = new RestaurantDbContext();
            var item = db.OrderItems.First(i => i.Id == SelectedOrderItem.Id);
            item.Quantity += delta;
            if (item.Quantity < 1) item.Quantity = 1;
            db.SaveChanges();

            SelectedOrderItem.Quantity = item.Quantity;
            UpdateTotals();
            DecreaseQtyCommand.RaiseCanExecuteChanged();
        }

        private void RemoveSelectedItem()
        {
            if (SelectedOrderItem == null) return;

            using var db = new RestaurantDbContext();
            var item = db.OrderItems.First(i => i.Id == SelectedOrderItem.Id);
            db.OrderItems.Remove(item);
            db.SaveChanges();

            CurrentItems.Remove(SelectedOrderItem);
            SelectedOrderItem = null;
            UpdateTotals();
            PayCommand.RaiseCanExecuteChanged();
        }

        private void Pay()
        {
            if (_currentOrder == null) return;
            using var db = new RestaurantDbContext();
            var order = db.Orders.First(o => o.Id == _currentOrder.Id);
            order.Status = "Paid";
            db.SaveChanges();

            CurrentItems.Clear();
            _currentOrder = null;
            UpdateTotals();
            UpdateRevenues(); // re-calculează încasările după plată
            PayCommand.RaiseCanExecuteChanged();
            AddItemCommand.RaiseCanExecuteChanged();
        }

        private void UpdateTotals()
        {
            Total = CurrentItems.Sum(i => i.UnitPrice * i.Quantity);
        }

        // --- FIX: fără DefaultIfEmpty; folosim Sum pe tip nullable și coalesce cu 0m
        private void UpdateRevenues()
        {
            using var db = new RestaurantDbContext();
            var today = DateTime.Today;

            RevenueToday = db.Orders
                .Where(o => o.Status == "Paid" && o.CreatedAt >= today)
                .SelectMany(o => o.Items)
                .Select(i => (decimal?)(i.UnitPrice * i.Quantity))
                .Sum() ?? 0m;

            RevenueAllTime = db.Orders
                .Where(o => o.Status == "Paid")
                .SelectMany(o => o.Items)
                .Select(i => (decimal?)(i.UnitPrice * i.Quantity))
                .Sum() ?? 0m;
        }
    }
}
