using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;
using TamAnh_EMR_System.Services;

namespace TamAnh_EMR_System.ViewModel
{
    public class AppointmentPanelViewModel : ViewModelBase
    {
        private readonly AppointmentPanelRepository repository;

        // ================= LIST =================
        public ObservableCollection<Appointment> Appointments { get; set; }

        private ObservableCollection<Appointment> _allAppointments;

        // ================= STATUS FILTER =================
        public ObservableCollection<string> StatusFilters { get; set; }

        private string _selectedStatusFilter;
        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                _selectedStatusFilter = value;

                OnPropertyChanged(nameof(SelectedStatusFilter));

                ApplyFilters();
            }
        }

        // ================= SELECTED =================
        private Appointment _selectedAppointment;
        public Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set
            {
                _selectedAppointment = value;

                OnPropertyChanged(nameof(SelectedAppointment));
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

        private Appointment _currentAppointment = new Appointment();
        public Appointment CurrentAppointment
        {
            get => _currentAppointment;
            set
            {
                _currentAppointment = value;

                OnPropertyChanged(nameof(CurrentAppointment));
            }
        }

        public string PopupTitle =>
            IsEditMode ? "Cập nhật lịch hẹn" : "Thêm lịch hẹn";

        // ================= COMMAND =================
        public ICommand LoadAppointmentCommand { get; }

        public ICommand OpenAddCommand { get; }

        public ICommand OpenEditCommand { get; }

        public ICommand SaveAppointmentCommand { get; }

        public ICommand DeleteAppointmentCommand { get; }

        public ICommand ClosePopupCommand { get; }

        public AppointmentPanelViewModel()
        {
            repository = new AppointmentPanelRepository();

            // ================= STATUS FILTER =================
            StatusFilters = new ObservableCollection<string>
            {
                "Tất cả",
                "Đang chờ",
                "Đang khám",
                "Hoàn thành",
                "Đã hủy"
            };

            SelectedStatusFilter = "Tất cả";

            // ================= LOAD =================
            LoadAppointmentCommand = new ViewModelCommand(_ =>
            {
                LoadAppointments();
            });

            // ================= ADD =================
            OpenAddCommand = new ViewModelCommand(_ =>
            {
                CurrentAppointment = new Appointment();

                IsEditMode = false;

                IsPopupOpen = true;
            });

            // ================= EDIT =================
            OpenEditCommand = new ViewModelCommand(obj =>
            {
                var appointment = obj as Appointment;

                if (appointment == null)
                    return;

                CurrentAppointment = new Appointment
                {
                    Id = appointment.Id,

                    PatientId = appointment.PatientId,

                    DoctorId = appointment.DoctorId,

                    CreatedBy = appointment.CreatedBy,

                    AppointmentDate = appointment.AppointmentDate,

                    AppointmentTime = appointment.AppointmentTime,

                    Status = appointment.Status,

                    Reason = appointment.Reason
                };

                IsEditMode = true;

                IsPopupOpen = true;
            });

            // ================= SAVE =================
            SaveAppointmentCommand = new ViewModelCommand(_ =>
            {
                if (!ValidateAppointment())
                    return;

                try
                {
                    MessageBox.Show("Chức năng lưu đang phát triển!");

                    LoadAppointments();

                    IsPopupOpen = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            // ================= DELETE =================
            DeleteAppointmentCommand = new ViewModelCommand(obj =>
            {
                var appointment = obj as Appointment;

                if (appointment == null)
                    return;

                if (MessageBox.Show(
                    "Bạn có chắc muốn xóa lịch hẹn?",
                    "Xác nhận",
                    MessageBoxButton.YesNo
                ) == MessageBoxResult.Yes)
                {
                    repository.DeleteAppointment(appointment.Id);

                    LoadAppointments();
                }
            });

            // ================= CLOSE =================
            ClosePopupCommand = new ViewModelCommand(_ =>
            {
                IsPopupOpen = false;
            });

            LoadAppointments();
        }

        // ================= VALIDATE =================
        private bool ValidateAppointment()
        {
            if (string.IsNullOrWhiteSpace(CurrentAppointment.PatientId))
            {
                MessageBox.Show("Chưa chọn bệnh nhân!");

                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentAppointment.DoctorId))
            {
                MessageBox.Show("Chưa chọn bác sĩ!");

                return false;
            }

            return true;
        }

        // ================= LOAD =================
        private void LoadAppointments()
        {
            _allAppointments =
                repository.GetAllAppointments();

            ApplyFilters();
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

                ApplyFilters();
            }
            catch (TaskCanceledException)
            {

            }
        }

        // ================= APPLY FILTER =================
        private void ApplyFilters()
        {
            if (_allAppointments == null)
                return;

            var filtered = _allAppointments.AsEnumerable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.ToLower();

                filtered = filtered.Where(a =>
                    (a.Id?.ToLower().Contains(keyword) ?? false) ||

                    (a.Patient?.Name?.ToLower().Contains(keyword) ?? false) ||

                    (a.Doctor?.FullName?.ToLower().Contains(keyword) ?? false) ||

                    (a.Status?.ToLower().Contains(keyword) ?? false) ||

                    (a.Reason?.ToLower().Contains(keyword) ?? false)
                );
            }

            // STATUS FILTER
            if (!string.IsNullOrWhiteSpace(SelectedStatusFilter)
                && SelectedStatusFilter != "Tất cả")
            {
                filtered = filtered.Where(a =>
                    a.Status == SelectedStatusFilter);
            }

            // SORT DESC DATE
            filtered = filtered
                .OrderByDescending(a => a.AppointmentDate);

            Appointments =
                new ObservableCollection<Appointment>(filtered);

            OnPropertyChanged(nameof(Appointments));
        }
    }
}