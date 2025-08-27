using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace RestaurantManager.Models
{
    public class MenuItem : INotifyPropertyChanged
    {
        private int _id;
        private string _name = "";
        private string _category = "";
        private decimal _price;
        private bool _isAvailable = true;

        public int Id
        {
            get => _id;
            set { if (_id != value) { _id = value; OnPropertyChanged(); } }
        }

        [Required, MaxLength(100)]
        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        [MaxLength(50)]
        public string Category
        {
            get => _category;
            set { if (_category != value) { _category = value; OnPropertyChanged(); } }
        }

        [Range(0, 10000)]
        public decimal Price
        {
            get => _price;
            set { if (_price != value) { _price = value; OnPropertyChanged(); } }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set { if (_isAvailable != value) { _isAvailable = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
