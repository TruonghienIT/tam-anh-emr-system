using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model.Doctor;

namespace TamAnh_EMR_System.ViewModel.Doctor
{
    /// <summary>
    /// ViewModel for Doctor Dashboard ("Trang chủ Bác sĩ").
    /// Provides stats, appointments, queue, and action commands.
    /// FUTURE: Replace LoadSampleData with API calls.
    /// </summary>
    public class DoctorDashboardViewModel : ViewModelBase
    {
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
            Appointments = new ObservableCollection<AppointmentItem>();
            QueuePatients = new ObservableCollection<QueuePatient>();
            ChartLine = new ObservableCollection<DoctorDashboardData>();
            ChartBars = new ObservableCollection<DoctorDashboardData>();

            RegisterPatientCommand = new RelayCommand(_ => MessageBox.Show("Đăng ký bệnh nhân", "Đăng ký"));
            CreateAppointmentCommand = new RelayCommand(_ => MessageBox.Show("Tạo lịch hẹn", "Lịch hẹn"));
            SearchPatientCommand = new RelayCommand(_ => MessageBox.Show("Tra cứu bệnh nhân", "Tra cứu"));
            CallPatientCommand = new RelayCommand(p =>
            {
                if (p is QueuePatient q) MessageBox.Show($"Gọi bệnh nhân: {q.Name}", "Gọi");
            });
            ViewAllCommand = new RelayCommand(_ => MessageBox.Show("Xem tất cả lịch hẹn", "Xem"));

            LoadSampleData();
        }

        private void LoadSampleData()
        {
            PatientsTodayCount = 42;
            DiseaseStatsCount = 15;
            QueueCount = 5;

            Appointments.Add(new AppointmentItem { Time = "09:00 AM", PatientName = "Nguyễn Văn A", DoctorName = "BS Lê tân", Status = "Đang chờ" });
            Appointments.Add(new AppointmentItem { Time = "09:30 AM", PatientName = "Trần Thị B", DoctorName = "BS Lê tân", Status = "Đã khám" });
            Appointments.Add(new AppointmentItem { Time = "10:00 AM", PatientName = "Lê Hoàng C", DoctorName = "BS Lê tân", Status = "Khẩn cấp" });

            QueuePatients.Add(new QueuePatient { Name = "Nguyễn Văn D", Initials = "NV", AvatarColor = "#6366F1", WaitingTime = "Waiting: 15 mins", IsUrgent = false });
            QueuePatients.Add(new QueuePatient { Name = "Phạm Thị E", Initials = "PT", AvatarColor = "#6366F1", WaitingTime = "Waiting: 22 mins", IsUrgent = false });
            QueuePatients.Add(new QueuePatient { Name = "Hoàng Minh F", Initials = "HM", AvatarColor = "#F59E0B", WaitingTime = "Waiting: 5 mins", IsUrgent = true });

            // Line chart data points
            for (int i = 0; i < 8; i++)
                ChartLine.Add(new DoctorDashboardData { Value = new[] { 10, 15, 12, 25, 30, 28, 35, 42 }[i] });

            // Bar chart data points
            ChartBars.Add(new DoctorDashboardData { Value = 30 });
            ChartBars.Add(new DoctorDashboardData { Value = 50 });
            ChartBars.Add(new DoctorDashboardData { Value = 70 });
            ChartBars.Add(new DoctorDashboardData { Value = 45 });
            ChartBars.Add(new DoctorDashboardData { Value = 60 });
        }
    }
}
