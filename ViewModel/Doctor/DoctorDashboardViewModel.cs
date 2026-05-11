using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model.Doctor;
using TamAnh_EMR_System.Repositories; // Đã đổi sang dùng thư mục Repositories của bạn

namespace TamAnh_EMR_System.ViewModel.Doctor
{
    /// <summary>
    /// ViewModel for Doctor Dashboard ("Trang chủ Bác sĩ").
    /// Provides stats, appointments, queue, and action commands.
    /// Dùng ADO.NET thông qua Repository Pattern.
    /// </summary>
    public class DoctorDashboardViewModel : ViewModelBase
    {
        // ===== REPOSITORY =====
        private readonly DoctorDashboardRepository _repository;

        // ===== STATS =====
        private int _patientsTodayCount;
        public int PatientsTodayCount
        {
            get => _patientsTodayCount;
            set { _patientsTodayCount = value; OnPropertyChanged(nameof(PatientsTodayCount)); }
        }

        private int _diseaseStatsCount;
        public int DiseaseStatsCount
        {
            get => _diseaseStatsCount;
            set { _diseaseStatsCount = value; OnPropertyChanged(nameof(DiseaseStatsCount)); }
        }

        private int _queueCount;
        public int QueueCount
        {
            get => _queueCount;
            set { _queueCount = value; OnPropertyChanged(nameof(QueueCount)); }
        }

        // ===== COLLECTIONS =====
        public ObservableCollection<AppointmentItem> Appointments { get; set; }
        public ObservableCollection<QueuePatient> QueuePatients { get; set; }
        public ObservableCollection<DoctorDashboardData> ChartLine { get; set; }
        public ObservableCollection<DoctorDashboardData> ChartBars { get; set; }

        // ===== COMMANDS =====
        public ICommand RegisterPatientCommand { get; }
        public ICommand CreateAppointmentCommand { get; }
        public ICommand SearchPatientCommand { get; }
        public ICommand CallPatientCommand { get; }
        public ICommand ViewAllCommand { get; }

        public DoctorDashboardViewModel()
        {
            // Khởi tạo Repository để gọi DB
            _repository = new DoctorDashboardRepository();

            Appointments = new ObservableCollection<AppointmentItem>();
            QueuePatients = new ObservableCollection<QueuePatient>();
            ChartLine = new ObservableCollection<DoctorDashboardData>();
            ChartBars = new ObservableCollection<DoctorDashboardData>();

            RegisterPatientCommand = new RelayCommand(_ => MessageBox.Show("Đăng ký bệnh nhân", "Đăng ký"));
            CreateAppointmentCommand = new RelayCommand(_ => MessageBox.Show("Tạo lịch hẹn", "Lịch hẹn"));
            SearchPatientCommand = new RelayCommand(_ => MessageBox.Show("Tra cứu bệnh nhân", "Tra cứu"));

            CallPatientCommand = new RelayCommand(p =>
            {
                if (p is QueuePatient q)
                {
                    // Tương lai: Update status trong DB từ "Đang chờ" -> "Đang khám" ở đây
                    MessageBox.Show($"Đang gọi bệnh nhân: {q.Name} vào phòng khám Tâm Anh.", "Gọi Bệnh Nhân");
                }
            });

            ViewAllCommand = new RelayCommand(_ => MessageBox.Show("Xem tất cả lịch hẹn", "Xem"));

            // Gọi hàm lấy dữ liệu bất đồng bộ từ DB
            _ = LoadDataFromDatabaseAsync();
        }

        private async Task LoadDataFromDatabaseAsync()
        {
            try
            {
                // 1. KÉO DỮ LIỆU TỪ DATABASE QUA REPOSITORY
                var todaysAppointments = await _repository.GetTodaysAppointmentsAsync();
                int totalDiseases = await _repository.GetTotalDiseasesCountAsync();

                // Lọc danh sách hàng đợi
                var queueList = todaysAppointments
                    .Where(a => a.Status == "Đang chờ" || a.Status == "Khẩn cấp")
                    .ToList();

                // 2. CẬP NHẬT GIAO DIỆN TRÊN UI THREAD (Vì đang chạy async)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Cập nhật các con số thống kê
                    PatientsTodayCount = todaysAppointments.Count;
                    QueueCount = queueList.Count;
                    DiseaseStatsCount = totalDiseases;

                    // Clear List cũ
                    Appointments.Clear();
                    QueuePatients.Clear();

                    // Đổ dữ liệu vào bảng Lịch Hẹn
                    foreach (var appt in todaysAppointments)
                    {
                        Appointments.Add(new AppointmentItem
                        {
                            Time = appt.AppointmentTime.ToString(@"hh\:mm"), // Lấy giờ:phút
                            PatientName = appt.PatientName,
                            DoctorName = appt.DoctorName,
                            Status = appt.Status
                        });
                    }

                    // Đổ dữ liệu vào Hàng Đợi
                    foreach (var q in queueList)
                    {
                        // Cắt chữ cái cuối cùng trong tên để làm Avatar
                        string pName = q.PatientName ?? "Unknown";
                        string initials = pName.Split(' ').LastOrDefault()?.Substring(0, 1).ToUpper() ?? "U";
                        bool isUrgent = q.Status == "Khẩn cấp";

                        QueuePatients.Add(new QueuePatient
                        {
                            Name = pName,
                            Initials = initials,
                            AvatarColor = isUrgent ? "#F59E0B" : "#6366F1", // Cam cho Khẩn cấp, Xanh cho bình thường
                            WaitingTime = isUrgent ? "Khẩn cấp" : "Đang chờ",
                            IsUrgent = isUrgent
                        });
                    }

                    // Tạm thời giữ Mock Data cho Biểu đồ (Chart)
                    LoadChartData();
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu: {ex.Message}", "Lỗi DB", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void LoadChartData()
        {
            ChartLine.Clear();
            ChartBars.Clear();

            // Line chart data points (Giả lập số bệnh nhân theo giờ)
            int[] lineValues = { 10, 15, 12, 25, 30, 28, 35, 42 };
            foreach (var val in lineValues)
            {
                ChartLine.Add(new DoctorDashboardData { Value = val });
            }

            // Bar chart data points (Giả lập số ca mắc các bệnh phổ biến)
            int[] barValues = { 30, 50, 70, 45, 60 };
            foreach (var val in barValues)
            {
                ChartBars.Add(new DoctorDashboardData { Value = val });
            }
        }
    }
}