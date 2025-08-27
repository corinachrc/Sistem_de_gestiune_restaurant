using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestaurantManager.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        private int _id;
        private int _orderId;
        private Order? _order;
        private int _menuItemId;
        private MenuItem? _menuItem;
        private int _quantity = 1;
        private decimal _unitPrice;

        public int Id { get => _id; set { if (_id != value) { _id = value; OnPropertyChanged(); } } }
        public int OrderId { get => _orderId; set { if (_orderId != value) { _orderId = value; OnPropertyChanged(); } } }
        public Order? Order { get => _order; set { if (_order != value) { _order = value; OnPropertyChanged(); } } }
        public int MenuItemId { get => _menuItemId; set { if (_menuItemId != value) { _menuItemId = value; OnPropertyChanged(); } } }
        public MenuItem? MenuItem { get => _menuItem; set { if (_menuItem != value) { _menuItem = value; OnPropertyChanged(); } } }

        public int Quantity
        {
            get => _quantity;
            set { if (_quantity != value) { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); } }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set { if (_unitPrice != value) { _unitPrice = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); } }
        }

        public decimal Subtotal => UnitPrice * Quantity;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
