using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel.Doctor
{
    public class DoctorAppointmentViewModel : ViewModelBase
    {
        private readonly AppointmentRepository _repo = new AppointmentRepository();

        public ObservableCollection<AppointmentDisplay> Appointments { get; set; } = new ObservableCollection<AppointmentDisplay>();

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate { get => _selectedDate; set { _selectedDate = value; OnPropertyChanged(nameof(SelectedDate)); _ = LoadAsync(); } }

        private AppointmentDisplay _selectedAppointment;
        public AppointmentDisplay SelectedAppointment { get => _selectedAppointment; set { _selectedAppointment = value; OnPropertyChanged(nameof(SelectedAppointment)); } }

        public ICommand RefreshCommand { get; }
        public ICommand ChangeStatusCommand { get; }
        public ICommand RescheduleCommand { get; }
        public ICommand OpenRecordCommand { get; }

        public DoctorAppointmentViewModel()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            ChangeStatusCommand = new RelayCommand(async p => await ChangeStatusAsync(p as AppointmentDisplay));
            RescheduleCommand = new RelayCommand(async p => await RescheduleAsync(p as AppointmentDisplay));
            OpenRecordCommand = new RelayCommand(p => OpenRecord(p as AppointmentDisplay));

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            try
            {
                var list = await _repo.GetAllDisplayAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Appointments.Clear();

                    // Show ALL appointments, ignore date filter (dates are visible in table columns anyway)
                    foreach (var a in list)
                    {
                        Appointments.Add(a);
                    }

                    // Debug: show count if empty
                    if (Appointments.Count == 0)
                    {
                        MessageBox.Show("Không có dữ liệu lịch hẹn từ cơ sở dữ liệu.\n\nKiểm tra:\n- Kết nối cơ sở dữ liệu\n- Có dữ liệu trong bảng appointments\n- Appointments có patient_id và doctor_id hợp lệ", 
                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải lịch hẹn: {ex.Message}\n\n{ex.StackTrace}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ChangeStatusAsync(AppointmentDisplay ap)
        {
            if (ap == null) return;

            // Cycle status: Đang chờ -> Đang khám -> Hoàn thành
            string next = ap.Status switch
            {
                "Đang chờ" => "Đang khám",
                "Đang khám" => "Hoàn thành",
                _ => "Hoàn thành"
            };

            try
            {
                await _repo.UpdateStatusAsync(ap.Id, next);
                ap.Status = next;
                OnPropertyChanged(nameof(Appointments));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể cập nhật trạng thái: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RescheduleAsync(AppointmentDisplay ap)
        {
            if (ap == null) return;

            // open reschedule window
            var win = new View.Doctor.RescheduleWindow(ap.AppointmentDate, ap.AppointmentTime);
            if (win.ShowDialog() == true)
            {
                DateTime newDate = win.SelectedDate;
                string newTime = win.SelectedTime;

                try
                {
                    await _repo.UpdateAppointmentDateTimeAsync(ap.Id, newDate, newTime);
                    ap.AppointmentDate = newDate;
                    ap.AppointmentTime = newTime;
                    OnPropertyChanged(nameof(Appointments));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi dời lịch: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenRecord(AppointmentDisplay ap)
        {
            if (ap == null) return;

            MessageBox.Show($"Mở hồ sơ bệnh án cho bệnh nhân {ap.PatientName} (ID: {ap.PatientId})", "Hồ sơ bệnh án");
            // Integration: could navigate to DoctorPatientManagementView and set SelectedPatient
        }
    }
}
