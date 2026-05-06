using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;
using TamAnh_EMR_System.Services;

namespace TamAnh_EMR_System.ViewModel
{
    public class ReceptionistPanelViewModel : ViewModelBase
    {
        private readonly ReceptionistPanelRepository repository;

        private readonly EmailService emailService = new EmailService();

        public ObservableCollection<Receptionists> Receptionists { get; set; }

        // ================= SELECTED =================
        private Receptionists _selectedReceptionist;
        public Receptionists SelectedReceptionist
        {
            get => _selectedReceptionist;
            set
            {
                _selectedReceptionist = value;
                OnPropertyChanged(nameof(SelectedReceptionist));
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

        private Receptionists _currentReceptionist = new Receptionists();
        public Receptionists CurrentReceptionist
        {
            get => _currentReceptionist;
            set
            {
                _currentReceptionist = value;
                OnPropertyChanged(nameof(CurrentReceptionist));
            }
        }

        public string PopupTitle => IsEditMode ? "Cập nhật lễ tân" : "Thêm lễ tân";

        // ================= COMMAND =================
        public ICommand LoadReceptionistCommand { get; }
        public ICommand OpenAddCommand { get; }
        public ICommand OpenEditCommand { get; }
        public ICommand SaveReceptionistCommand { get; }
        public ICommand DeleteReceptionistCommand { get; }
        public ICommand ClosePopupCommand { get; }

        public ReceptionistPanelViewModel()
        {
            repository = new ReceptionistPanelRepository();

            LoadReceptionistCommand = new ViewModelCommand(_ => LoadReceptionists());

            // ADD
            OpenAddCommand = new ViewModelCommand(_ =>
            {
                CurrentReceptionist = new Receptionists();
                IsEditMode = false;
                IsPopupOpen = true;
            });

            // EDIT
            OpenEditCommand = new ViewModelCommand(obj =>
            {
                var receptionist = obj as Receptionists;
                if (receptionist == null) return;

                CurrentReceptionist = new Receptionists
                {
                    Id = receptionist.Id,
                    UserId = receptionist.UserId,
                    FullName = receptionist.FullName,
                    Email = receptionist.Email,
                    Phone = receptionist.Phone
                };

                IsEditMode = true;
                IsPopupOpen = true;
            });

            // SAVE
            SaveReceptionistCommand = new ViewModelCommand(async _ =>
            {
                if (!ValidateReceptionist())
                    return;

                try
                {
                    if (IsEditMode)
                    {
                        repository.UpdateReceptionist(CurrentReceptionist);
                        MessageBox.Show("Cập nhật thành công!");
                    }
                    else
                    {
                        var result = repository.AddReceptionist(CurrentReceptionist);

                        await emailService.SendAccountEmailAsync(
                            CurrentReceptionist.Email,
                            result.username,
                            result.password, "receptionist"
                        );

                        MessageBox.Show(
                            $"Tạo lễ tân thành công!\nUsername: {result.username}\nPassword: {result.password}"
                        );
                    }

                    LoadReceptionists();
                    IsPopupOpen = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            // DELETE
            DeleteReceptionistCommand = new ViewModelCommand(obj =>
            {
                var receptionist = obj as Receptionists;
                if (receptionist == null) return;

                if (MessageBox.Show("Bạn có chắc muốn xóa?", "Xác nhận",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    repository.DeleteReceptionist(receptionist.UserId);
                    LoadReceptionists();
                }
            });

            // CLOSE
            ClosePopupCommand = new ViewModelCommand(_ =>
            {
                IsPopupOpen = false;
            });

            LoadReceptionists();
        }

        // ================= VALIDATE =================
        private bool ValidateReceptionist()
        {
            if (string.IsNullOrWhiteSpace(CurrentReceptionist.FullName))
            {
                MessageBox.Show("Nhập họ tên!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentReceptionist.Email))
            {
                MessageBox.Show("Nhập email!");
                return false;
            }

            return true;
        }

        // ================= LOAD =================
        private void LoadReceptionists()
        {
            Receptionists = repository.GetAllReceptionists();
            OnPropertyChanged(nameof(Receptionists));
        }

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

        private async void SearchAsync()
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                await Task.Delay(300, _cts.Token);

                if (string.IsNullOrWhiteSpace(_searchText))
                {
                    LoadReceptionists();
                }
                else
                {
                    Receptionists = repository.SearchReceptionists(_searchText);
                    OnPropertyChanged(nameof(Receptionists));
                }
            }
            catch (TaskCanceledException) { }
        }
    }
}