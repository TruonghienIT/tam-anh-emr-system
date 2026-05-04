using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel
{
    public class UserPanelViewModel : ViewModelBase
    {
        private UserRepository repository;

        // ================= LIST =================
        public ObservableCollection<Users> Users { get; set; }

        // ================= SELECTED =================
        private Users _selectedUser;
        public Users SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }

        // ================= POPUP =================
        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                _isPopupOpen = value;
                OnPropertyChanged(nameof(IsPopupOpen));
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged(nameof(IsEditMode));
                OnPropertyChanged(nameof(PopupTitle));
            }
        }

        private Users _currentUser = new Users();
        public Users CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        public string PopupTitle => IsEditMode ? "Cập nhật tài khoản" : "Thêm người dùng";

        // ================= COMMAND =================
        public ICommand LoadUserCommand { get; }
        public ICommand OpenAddCommand { get; }
        public ICommand OpenEditCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ClosePopupCommand { get; }

        // ================= SEARCH =================
        private string _searchText;
        private CancellationTokenSource _cts;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                SearchAsync();
            }
        }

        // ================= CONSTRUCTOR =================
        public UserPanelViewModel()
        {
            repository = new UserRepository();

            LoadUserCommand = new ViewModelCommand(_ => LoadUsers());

            // ===== ADD =====
            OpenAddCommand = new ViewModelCommand(_ =>
            {
                CurrentUser = new Users();
                IsEditMode = false;
                IsPopupOpen = true;
            });

            // ===== EDIT =====
            OpenEditCommand = new ViewModelCommand(obj =>
            {
                var user = obj as Users;
                if (user == null) return;

                CurrentUser = new Users
                {
                    Id = user.Id,
                    Username = user.Username,
                    Password = user.Password,
                    Role = user.Role
                };

                IsEditMode = true;
                IsPopupOpen = true;
            });

            // ===== SAVE =====
            SaveUserCommand = new ViewModelCommand(_ =>
            {
                if (!ValidateUser())
                    return;

                try
                {
                    if (IsEditMode)
                        repository.Edit(CurrentUser);
                    else
                        repository.Add(CurrentUser);

                    LoadUsers();
                    IsPopupOpen = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            // ===== DELETE =====
            DeleteUserCommand = new ViewModelCommand(obj =>
            {
                var user = obj as Users;
                if (user == null) return;

                if (MessageBox.Show("Bạn có chắc muốn xóa?", "Xác nhận",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    repository.Remove(int.Parse(user.Id));
                    LoadUsers();
                }
            });

            // ===== CLOSE POPUP =====
            ClosePopupCommand = new ViewModelCommand(_ =>
            {
                IsPopupOpen = false;
            });

            LoadUsers();
        }

        // ================= LOAD =================
        private void LoadUsers()
        {
            Users = new ObservableCollection<Users>(repository.GetByAll());
            OnPropertyChanged(nameof(Users));
        }

        // ================= SEARCH =================
        private async void SearchAsync()
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                await Task.Delay(300, _cts.Token);

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    LoadUsers();
                }
                else
                {
                    var all = repository.GetByAll();

                    Users = new ObservableCollection<Users>(
                        all as System.Collections.Generic.IEnumerable<Users>
                        ?? all
                    );

                    Users = new ObservableCollection<Users>(
                        Users as System.Collections.Generic.IEnumerable<Users>
                    );

                    Users = new ObservableCollection<Users>(
                        System.Linq.Enumerable.Where(all, x =>
                            x.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                            x.Role.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                        )
                    );

                    OnPropertyChanged(nameof(Users));
                }
            }
            catch (TaskCanceledException) { }
        }

        // ================= VALIDATE =================
        private bool ValidateUser()
        {
            if (string.IsNullOrWhiteSpace(CurrentUser.Username))
            {
                MessageBox.Show("Vui lòng nhập username!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                MessageBox.Show("Vui lòng nhập password!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentUser.Role))
            {
                MessageBox.Show("Vui lòng nhập role!");
                return false;
            }

            return true;
        }
    }

}