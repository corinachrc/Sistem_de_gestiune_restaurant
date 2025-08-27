using System.Collections.ObjectModel;
using System.Linq;
using RestaurantManager.Data;
using RestaurantManager.MVVM;
using RestaurantManager.Models;

namespace RestaurantManager.ViewModels
{
    public class UsersViewModel : BaseViewModel
    {
        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<string> Roles { get; } = new() { "Admin", "Waiter" };

        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    DeactivateCommand.RaiseCanExecuteChanged();
                    StartEditCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // Formular ADD – gol la start
        private User _newUser = new() { Username = "", Password = "", Role = "Waiter", IsActive = true };
        public User NewUser
        {
            get => _newUser;
            set
            {
                if (SetProperty(ref _newUser, value))
                {
                    AddCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // Mod editare
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

        private User _editUser = new();
        public User EditUser
        {
            get => _editUser;
            set => SetProperty(ref _editUser, value);
        }

        // Comenzi
        public RelayCommand LoadCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand DeactivateCommand { get; }
        public RelayCommand StartEditCommand { get; }
        public RelayCommand SaveEditCommand { get; }
        public RelayCommand CancelEditCommand { get; }

        public UsersViewModel()
        {
            LoadCommand = new RelayCommand(_ => Load());

            AddCommand = new RelayCommand(_ => Add(),
                                     _ => !string.IsNullOrWhiteSpace(NewUser.Username)
                                       && !string.IsNullOrWhiteSpace(NewUser.Password));

            DeactivateCommand = new RelayCommand(_ => Deactivate(), _ => SelectedUser != null);

            StartEditCommand = new RelayCommand(_ => StartEdit(), _ => SelectedUser != null);

            SaveEditCommand = new RelayCommand(_ => SaveEdit(), _ => IsEditing);
            CancelEditCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);

            Load();
        }

        // Încarcă utilizatorii: întâi activii, apoi inactivii; opțional selectează un ID
        private void Load(int? selectUserId = null)
        {
            Users.Clear();
            using var db = new RestaurantDbContext();
            var list = db.Users
                         .OrderByDescending(u => u.IsActive) // activii sus
                         .ThenBy(u => u.Username)
                         .ToList();
            foreach (var u in list) Users.Add(u);

            if (selectUserId.HasValue)
                SelectedUser = Users.FirstOrDefault(u => u.Id == selectUserId.Value);
        }

        private void Add()
        {
            using var db = new RestaurantDbContext();

            if (db.Users.Any(u => u.Username == NewUser.Username))
                return;

            db.Users.Add(NewUser);
            db.SaveChanges();

            var newId = NewUser.Id;
            // Reset form
            NewUser = new User { Username = "", Password = "", Role = "Waiter", IsActive = true };

            // Reîncarcă și selectează utilizatorul creat (listă resortată corect)
            Load(newId);
        }

        // „Ștergere” logică → IsActive = false
        private void Deactivate()
        {
            if (SelectedUser is null) return;

            using var db = new RestaurantDbContext();
            var u = db.Users.Find(SelectedUser.Id);
            if (u == null) return;

            u.IsActive = false;
            db.SaveChanges();

            // Reîncarcă lista ca să apară jos printre inactivi
            Load(u.Id);
        }

        private void StartEdit()
        {
            if (SelectedUser == null) return;

            // Copie „buffer” pentru editare
            EditUser = new User
            {
                Id = SelectedUser.Id,
                Username = SelectedUser.Username,
                Password = "", // lăsat gol; dacă rămâne gol la salvare, nu schimbăm parola
                Role = SelectedUser.Role,
                IsActive = SelectedUser.IsActive
            };
            IsEditing = true;
        }

        private void SaveEdit()
        {
            if (!IsEditing) return;

            using var db = new RestaurantDbContext();
            var u = db.Users.FirstOrDefault(x => x.Id == EditUser.Id);
            if (u == null) return;

            u.Username = EditUser.Username;
            u.Role = EditUser.Role;
            u.IsActive = EditUser.IsActive;
            if (!string.IsNullOrWhiteSpace(EditUser.Password))
                u.Password = EditUser.Password;

            db.SaveChanges();

            // Reîncarcă lista (cu ordonare corectă) și selectează utilizatorul modificat
            var id = u.Id;
            IsEditing = false;
            EditUser = new User();
            Load(id);
        }

        private void CancelEdit()
        {
            IsEditing = false;
            EditUser = new User();
        }
    }
}
