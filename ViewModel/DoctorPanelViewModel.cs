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
    public class DoctorPanelViewModel : ViewModelBase
    {
        private readonly DoctorPanelRepository repository;

        private readonly EmailService emailService = new EmailService();

        public ObservableCollection<Doctors> Doctors { get; set; }

        // ================= SELECTED =================
        private Doctors _selectedDoctor;
        public Doctors SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                _selectedDoctor = value;
                OnPropertyChanged(nameof(SelectedDoctor));
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

        private Doctors _currentDoctor = new Doctors();
        public Doctors CurrentDoctor
        {
            get => _currentDoctor;
            set
            {
                _currentDoctor = value;
                OnPropertyChanged(nameof(CurrentDoctor));
            }
        }

        public string PopupTitle => IsEditMode ? "Cập nhật bác sĩ" : "Thêm bác sĩ";

        // ================= COMMAND =================
        public ICommand LoadDoctorCommand { get; }
        public ICommand OpenAddCommand { get; }
        public ICommand OpenEditCommand { get; }
        public ICommand SaveDoctorCommand { get; }
        public ICommand DeleteDoctorCommand { get; }
        public ICommand ClosePopupCommand { get; }

        public DoctorPanelViewModel()
        {
            repository = new DoctorPanelRepository();

            LoadDoctorCommand = new ViewModelCommand(_ => LoadDoctors());

            // ADD
            OpenAddCommand = new ViewModelCommand(_ =>
            {
                CurrentDoctor = new Doctors();
                IsEditMode = false;
                IsPopupOpen = true;
            });

            // EDIT
            OpenEditCommand = new ViewModelCommand(obj =>
            {
                var doctor = obj as Doctors;
                if (doctor == null) return;

                CurrentDoctor = new Doctors
                {
                    Id = doctor.Id,
                    UserId = doctor.UserId,
                    FullName = doctor.FullName,
                    Email = doctor.Email,
                    Phone = doctor.Phone,
                    Specialization = doctor.Specialization
                };

                IsEditMode = true;
                IsPopupOpen = true;
            });

            // SAVE
            SaveDoctorCommand = new ViewModelCommand(async _ =>
            {
                if (!ValidateDoctor())
                    return;

                try
                {
                    if (IsEditMode)
                    {
                        repository.UpdateDoctor(CurrentDoctor);
                        MessageBox.Show("Cập nhật thành công!");
                    }
                    else
                    {
                        var result = repository.AddDoctor(CurrentDoctor);

                        await emailService.SendAccountEmailAsync(
                            CurrentDoctor.Email,
                            result.username,
                            result.password, "doctor"
                        );

                        MessageBox.Show(
                            $"Tạo bác sĩ thành công!\nUsername: {result.username}\nPassword: {result.password}"
                        );
                    }

                    LoadDoctors();
                    IsPopupOpen = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            // DELETE
            DeleteDoctorCommand = new ViewModelCommand(obj =>
            {
                var doctor = obj as Doctors;
                if (doctor == null) return;

                if (MessageBox.Show("Bạn có chắc muốn xóa?", "Xác nhận",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    repository.DeleteDoctor(doctor.UserId);
                    LoadDoctors();
                }
            });

            // CLOSE
            ClosePopupCommand = new ViewModelCommand(_ =>
            {
                IsPopupOpen = false;
            });

            LoadDoctors();
        }

        // ================= VALIDATE =================
        private bool ValidateDoctor()
        {
            if (string.IsNullOrWhiteSpace(CurrentDoctor.FullName))
            {
                MessageBox.Show("Nhập họ tên!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentDoctor.Email))
            {
                MessageBox.Show("Nhập email!");
                return false;
            }

            return true;
        }

        // ================= LOAD =================
        private void LoadDoctors()
        {
            Doctors = repository.GetAllDoctors();
            OnPropertyChanged(nameof(Doctors));
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
                    LoadDoctors();
                }
                else
                {
                    Doctors = repository.SearchDoctors(_searchText);
                    OnPropertyChanged(nameof(Doctors));
                }
            }
            catch (TaskCanceledException) { }
        }
    }
}