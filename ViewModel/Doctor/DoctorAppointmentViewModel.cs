using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private List<AppointmentDisplay> _allAppointments = new List<AppointmentDisplay>();

        public ObservableCollection<AppointmentDisplay> Appointments { get; set; } = new ObservableCollection<AppointmentDisplay>();


        private DateTime _fromDate = DateTime.Today;
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged(nameof(FromDate));
                _ = FilterAppointmentsAsync();
            }
        }

        private DateTime _toDate = DateTime.Today;
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged(nameof(ToDate));
                _ = FilterAppointmentsAsync();
            }
        }
        private string _statusFilter = "Tất cả";
        public string StatusFilter
        {
            get => _statusFilter;
            set
            {
                _statusFilter = value;
                OnPropertyChanged(nameof(StatusFilter));
                _ = FilterAppointmentsAsync();
            }
        }

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
                    // Lưu tất cả dữ liệu, sắp xếp mới nhất lên đầu
                    _allAppointments = list.OrderByDescending(a => a.AppointmentDate)
                                          .ThenByDescending(a => a.AppointmentTime)
                                          .ToList();

                    // Áp dụng filter
                    _ = FilterAppointmentsAsync();

                    // Debug: show count if empty
                    if (_allAppointments.Count == 0)
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

        private async Task FilterAppointmentsAsync()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Appointments.Clear();

                    var filtered = _allAppointments.AsEnumerable();

                    // 1. Lọc theo khoảng thời gian (Từ ngày -> Đến ngày)
                    filtered = filtered.Where(a => a.AppointmentDate.Date >= FromDate.Date && a.AppointmentDate.Date <= ToDate.Date);

                    // 2. Lọc theo trạng thái
                    if (_statusFilter != "Tất cả")
                    {
                        filtered = filtered.Where(a => a.Status == _statusFilter);
                    }

                    // Đẩy vào danh sách hiển thị
                    foreach (var a in filtered.OrderByDescending(x => x.AppointmentDate)
                                              .ThenByDescending(x => x.AppointmentTime))
                    {
                        Appointments.Add(a);
                    }
                });
            });
        }

        private string MapStatusFilter(string filterValue)
        {
            return filterValue switch
            {
                "Đang chờ" => "Pending",
                "Đang khám" => "Confirmed",
                "Hoàn thành" => "Completed",
                "Đã hủy" => "Cancelled",
                _ => "Pending"
            };
        }

        private async Task ChangeStatusAsync(AppointmentDisplay ap)
        {
            if (ap == null) return;

            // Cycle status: Pending -> Confirmed -> Completed
            string next = ap.Status switch
            {
                "Pending" => "Confirmed",
                "Confirmed" => "Completed",
                _ => "Completed"
            };

            try
            {
                await _repo.UpdateStatusAsync(ap.Id, next);
                ap.Status = next;
                OnPropertyChanged(nameof(Appointments));

                // Reload để cập nhật vị trí
                _ = LoadAsync();
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

                    // Reload để cập nhật sắp xếp
                    _ = LoadAsync();
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
