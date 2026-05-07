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
    public class PatientPanelViewModel : ViewModelBase
    {
        private readonly PatientPanelRepository repository;

        private ObservableCollection<Patients> _patients;
        public ObservableCollection<Patients> Patients
        {
            get => _patients;
            set
            {
                _patients = value;
                OnPropertyChanged(nameof(Patients));
            }
        }

        private Patients _selectedPatient;
        public Patients SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged(nameof(SelectedPatient));
            }
        }

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

        private bool _isViewMode;
        public bool IsViewMode
        {
            get => _isViewMode;
            set
            {
                _isViewMode = value;
                OnPropertyChanged(nameof(IsViewMode));
                OnPropertyChanged(nameof(PopupTitle));
            }
        }

        private Patients _currentPatient = new Patients();
        public Patients CurrentPatient
        {
            get => _currentPatient;
            set
            {
                _currentPatient = value;
                OnPropertyChanged(nameof(CurrentPatient));
            }
        }

        public string PopupTitle
        {
            get
            {
                if (IsViewMode)
                    return "Thông tin bệnh nhân";
                if (IsEditMode)
                    return "Cập nhật bệnh nhân";
                return "Thêm bệnh nhân";
            }
        }

        public ICommand OpenAddCommand { get; }
        public ICommand OpenViewCommand { get; }
        public ICommand OpenEditCommand { get; }
        public ICommand SavePatientCommand { get; }
        public ICommand DeletePatientCommand { get; }
        public ICommand ClosePopupCommand { get; }

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

        public PatientPanelViewModel()
        {
            repository = new PatientPanelRepository();

            OpenAddCommand = new ViewModelCommand(_ =>
            {
                CurrentPatient = new Patients
                {
                    Dob = DateTime.Now
                };

                IsEditMode = false;
                IsViewMode = false;
                IsPopupOpen = true;
            });

            OpenViewCommand = new ViewModelCommand(obj =>
            {
                var patient = obj as Patients;
                if (patient == null)
                    return;

                CurrentPatient = ClonePatient(patient);

                IsViewMode = true;
                IsEditMode = false;
                IsPopupOpen = true;
            });

            OpenEditCommand = new ViewModelCommand(obj =>
            {
                var patient = obj as Patients;
                if (patient == null)
                    return;

                CurrentPatient = ClonePatient(patient);

                IsViewMode = false;
                IsEditMode = true;
                IsPopupOpen = true;
            });

            SavePatientCommand = new ViewModelCommand(_ =>
            {
                if (IsViewMode)
                    return;

                if (!ValidatePatient())
                    return;

                try
                {
                    if (IsEditMode)
                    {
                        repository.UpdatePatient(CurrentPatient);
                        MessageBox.Show(
                            "Cập nhật bệnh nhân thành công!",
                            "Thông báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        repository.AddPatient(CurrentPatient);
                        MessageBox.Show(
                            "Thêm bệnh nhân thành công!",
                            "Thông báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }

                    LoadPatients();
                    IsPopupOpen = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            });

            DeletePatientCommand = new ViewModelCommand(obj =>
            {
                var patient = obj as Patients;

                if (patient == null)
                    return;

                var result = MessageBox.Show(
                    $"Bạn có chắc muốn xóa bệnh nhân '{patient.Name}'?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        repository.DeletePatient(patient.Id);

                        MessageBox.Show(
                            "Xóa bệnh nhân thành công!",
                            "Thông báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        LoadPatients();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            ex.Message,
                            "Lỗi",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            });

            ClosePopupCommand = new ViewModelCommand(_ =>
            {
                IsPopupOpen = false;
            });

            LoadPatients();
        }

        private Patients ClonePatient(Patients patient)
        {
            return new Patients
            {
                Id = patient.Id,
                UserId = patient.UserId,
                Name = patient.Name,
                Dob = patient.Dob,
                Gender = patient.Gender,
                Address = patient.Address,
                Phone = patient.Phone,
                Email = patient.Email,
                IdCard = patient.IdCard,
                BloodType = patient.BloodType,
                Allergies = patient.Allergies,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone
            };
        }

        private bool ValidatePatient()
        {
            if (string.IsNullOrWhiteSpace(CurrentPatient.Name))
            {
                MessageBox.Show(
                    "Vui lòng nhập họ tên!",
                    "Thiếu thông tin",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            if (CurrentPatient.Dob == default)
            {
                MessageBox.Show(
                    "Vui lòng chọn ngày sinh!",
                    "Thiếu thông tin",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentPatient.Gender))
            {
                MessageBox.Show(
                    "Vui lòng chọn giới tính!",
                    "Thiếu thông tin",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentPatient.Phone))
            {
                MessageBox.Show(
                    "Vui lòng nhập số điện thoại!",
                    "Thiếu thông tin",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void LoadPatients()
        {
            try
            {
                Patients = repository.GetAllPatients();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi tải dữ liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
                    LoadPatients();
                }
                else
                {
                    Patients = repository.SearchPatients(_searchText);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi tìm kiếm",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}