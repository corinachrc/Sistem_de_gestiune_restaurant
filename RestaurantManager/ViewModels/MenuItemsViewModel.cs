using System.Collections.ObjectModel;
using System.Linq;
using RestaurantManager.Data;
using RestaurantManager.MVVM;
using RestaurantManager.Models;

namespace RestaurantManager.ViewModels
{
    public class MenuItemsViewModel : BaseViewModel
    {
        public ObservableCollection<MenuItem> Items { get; } = new();

        private MenuItem? _selectedItem;
        public MenuItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    DeactivateCommand.RaiseCanExecuteChanged();
                    StartEditCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // formular ADĂUGARE
        private MenuItem _newItem = new() { Name = "", Category = "", Price = 0m, IsAvailable = true };
        public MenuItem NewItem
        {
            get => _newItem;
            set
            {
                if (SetProperty(ref _newItem, value))
                {
                    AddCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // formular EDITARE
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    SaveEditCommand.RaiseCanExecuteChanged();
                    CancelEditCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private MenuItem _editItem = new();
        public MenuItem EditItem
        {
            get => _editItem;
            set => SetProperty(ref _editItem, value);
        }

        public RelayCommand LoadCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand DeactivateCommand { get; }
        public RelayCommand StartEditCommand { get; }
        public RelayCommand SaveEditCommand { get; }
        public RelayCommand CancelEditCommand { get; }

        public MenuItemsViewModel()
        {
            LoadCommand = new RelayCommand(_ => Load());
            AddCommand = new RelayCommand(_ => Add(),
                                   _ => !string.IsNullOrWhiteSpace(NewItem.Name) && NewItem.Price > 0m);
            DeactivateCommand = new RelayCommand(_ => Deactivate(), _ => SelectedItem != null);
            StartEditCommand = new RelayCommand(_ => StartEdit(), _ => SelectedItem != null);
            SaveEditCommand = new RelayCommand(_ => SaveEdit(), _ => IsEditing);
            CancelEditCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);

            Load();
        }

        // Activele primele, apoi inactivele
        private void Load(int? selectId = null)
        {
            Items.Clear();
            using var db = new RestaurantDbContext();
            var list = db.MenuItems
                         .OrderByDescending(m => m.IsAvailable)
                         .ThenBy(m => m.Category)
                         .ThenBy(m => m.Name)
                         .ToList();
            foreach (var m in list) Items.Add(m);

            if (selectId.HasValue)
                SelectedItem = Items.FirstOrDefault(i => i.Id == selectId.Value);
        }

        private void Add()
        {
            using var db = new RestaurantDbContext();
            db.MenuItems.Add(NewItem);
            db.SaveChanges();
            var newId = NewItem.Id;

            // reset formular
            NewItem = new MenuItem { Name = "", Category = "", Price = 0m, IsAvailable = true };

            Load(newId);
            AppEvents.RaiseMenuChanged(); // << anunțăm tabul Comenzi
        }

        // dezactivare logică
        private void Deactivate()
        {
            if (SelectedItem == null) return;

            using var db = new RestaurantDbContext();
            var entity = db.MenuItems.Find(SelectedItem.Id);
            if (entity == null) return;

            entity.IsAvailable = false;
            db.SaveChanges();

            Load(entity.Id);
            AppEvents.RaiseMenuChanged(); // << anunțăm tabul Comenzi
        }

        private void StartEdit()
        {
            if (SelectedItem == null) return;

            EditItem = new MenuItem
            {
                Id = SelectedItem.Id,
                Name = SelectedItem.Name,
                Category = SelectedItem.Category,
                Price = SelectedItem.Price,
                IsAvailable = SelectedItem.IsAvailable
            };
            IsEditing = true;
        }

        private void SaveEdit()
        {
            if (!IsEditing) return;

            using var db = new RestaurantDbContext();
            var entity = db.MenuItems.FirstOrDefault(x => x.Id == EditItem.Id);
            if (entity == null) return;

            entity.Name = EditItem.Name;
            entity.Category = EditItem.Category;
            entity.Price = EditItem.Price;
            entity.IsAvailable = EditItem.IsAvailable;

            db.SaveChanges();

            var id = entity.Id;
            IsEditing = false;
            EditItem = new MenuItem();
            Load(id);
            AppEvents.RaiseMenuChanged(); // << anunțăm tabul Comenzi
        }

        private void CancelEdit()
        {
            IsEditing = false;
            EditItem = new MenuItem();
        }
    }
}
