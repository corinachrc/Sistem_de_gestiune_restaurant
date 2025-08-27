using System.Collections.ObjectModel;
using System.Linq;
using RestaurantManager.Data;
using RestaurantManager.Models;
using RestaurantManager.MVVM;

namespace RestaurantManager.ViewModels
{
    public class MenuItemsViewModel : BaseViewModel
    {
        private MenuItem? _selectedItem;
        private MenuItem _newItem = new();

        public ObservableCollection<MenuItem> Items { get; } = new();

        public MenuItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    DeleteCommand.RaiseCanExecuteChanged();
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public MenuItem NewItem
        {
            get => _newItem;
            set
            {
                if (SetProperty(ref _newItem, value))
                {
                    _newItem.PropertyChanged -= NewItem_PropertyChanged;
                    _newItem.PropertyChanged += NewItem_PropertyChanged;
                    AddCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public RelayCommand LoadCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }

        public MenuItemsViewModel()
        {
            LoadCommand = new RelayCommand(_ => Load());
            AddCommand = new RelayCommand(_ => Add(), _ => !string.IsNullOrWhiteSpace(NewItem.Name) && NewItem.Price > 0);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedItem != null);
            SaveCommand = new RelayCommand(_ => Save(), _ => SelectedItem != null);

            _newItem.PropertyChanged += NewItem_PropertyChanged;
            Load();
        }

        private void NewItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            => AddCommand.RaiseCanExecuteChanged();

        private void Load()
        {
            Items.Clear();
            using var db = new RestaurantDbContext();
            foreach (var m in db.MenuItems.OrderBy(m => m.Category).ThenBy(m => m.Name))
                Items.Add(m);
        }

        private void Add()
        {
            using var db = new RestaurantDbContext();
            db.MenuItems.Add(NewItem);
            db.SaveChanges();

            Items.Add(NewItem);
            NewItem = new MenuItem();
            NewItem.PropertyChanged += NewItem_PropertyChanged;
        }

        private void Delete()
        {
            if (SelectedItem is null) return;
            using var db = new RestaurantDbContext();
            var found = db.MenuItems.Find(SelectedItem.Id);
            if (found != null)
            {
                db.MenuItems.Remove(found);
                db.SaveChanges();
            }
            Items.Remove(SelectedItem);
            SelectedItem = null;
        }

        private void Save()
        {
            if (SelectedItem is null) return;
            using var db = new RestaurantDbContext();
            db.MenuItems.Update(SelectedItem);
            db.SaveChanges();
            Load();
        }
    }
}
