using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Helper;
using TamAnh_EMR_System.Model.Doctor;
using TamAnh_EMR_System.Repositories;
using TamAnh_EMR_System.View;

namespace TamAnh_EMR_System.ViewModel.Doctor
{
    public class DoctorDashboardViewModel : ViewModelBase
    {
        private readonly DoctorDashboardRepository _repository;

        private readonly DispatcherTimer _refreshTimer;

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

        public ObservableCollection<AppointmentItem> Appointments { get; set; }
        public ObservableCollection<QueuePatient> QueuePatients { get; set; }
        public ObservableCollection<DoctorDashboardData> ChartLine { get; set; }
        public ObservableCollection<DoctorDashboardData> ChartBars { get; set; }

        public ICommand RegisterPatientCommand { get; }
        public ICommand CreateAppointmentCommand { get; }
        public ICommand SearchPatientCommand { get; }
        public ICommand CallPatientCommand { get; }
        public ICommand ViewAllCommand { get; }

        public ICommand LogoutCommand { get; }

        public DoctorDashboardViewModel()
        {
            _repository = new DoctorDashboardRepository();

            Appointments = new ObservableCollection<AppointmentItem>();
            QueuePatients = new ObservableCollection<QueuePatient>();
            ChartLine = new ObservableCollection<DoctorDashboardData>();
            ChartBars = new ObservableCollection<DoctorDashboardData>();

            RegisterPatientCommand = new RelayCommand(_ => MessageBox.Show("Đăng ký bệnh nhân", "Đăng ký"));
            CreateAppointmentCommand = new RelayCommand(_ => MessageBox.Show("Tạo lịch hẹn", "Lịch hẹn"));
            SearchPatientCommand = new RelayCommand(_ => MessageBox.Show("Tra cứu bệnh nhân", "Tra cứu"));

            LogoutCommand = new ViewModelCommand(ExecuteLogoutCommand);

            CallPatientCommand = new RelayCommand(p =>
            {
                if (p is QueuePatient q)
                {
                    MessageBox.Show($"Đang gọi bệnh nhân: {q.Name} vào phòng khám Tâm Anh.", "Gọi Bệnh Nhân");
                }
            });

            ViewAllCommand = new RelayCommand(_ => MessageBox.Show("Xem tất cả lịch hẹn", "Xem"));

            _ = LoadDataFromDatabaseAsync();

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };

            _refreshTimer.Tick += async (s, e) =>
            {
                await LoadDataFromDatabaseAsync();
            };

            _refreshTimer.Start();
        }

        private async Task LoadDataFromDatabaseAsync()
        {
            try
            {
                var todaysAppointments = await _repository.GetTodaysAppointmentsAsync();
                int totalDiseases = await _repository.GetTotalDiseasesCountAsync();

                var activeQueueList = todaysAppointments
                    .Where(a =>
                        a.Status == "Đang chờ" ||
                        a.Status == "Đang khám")
                    .OrderBy(a => a.AppointmentTime)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    PatientsTodayCount = todaysAppointments.Count;
                    QueueCount = activeQueueList.Count;
                    DiseaseStatsCount = totalDiseases;

                    Appointments.Clear();
                    QueuePatients.Clear();

                    foreach (var appt in todaysAppointments.OrderBy(a => a.AppointmentTime))
                    {
                        Appointments.Add(new AppointmentItem
                        {
                            Time = appt.AppointmentTime.ToString(@"hh\:mm"),
                            PatientName = appt.PatientName ?? "Không xác định",
                            DoctorName = appt.DoctorName ?? "Không xác định",
                            Status = appt.Status ?? "Không rõ"
                        });
                    }

                    foreach (var q in activeQueueList)
                    {
                        string patientName = string.IsNullOrWhiteSpace(q.PatientName)
                            ? "Unknown"
                            : q.PatientName;

                        string initials = patientName
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .LastOrDefault()?
                            .Substring(0, 1)
                            .ToUpper() ?? "U";

                        bool isExamining = q.Status == "Đang khám";

                        QueuePatients.Add(new QueuePatient
                        {
                            Name = patientName,
                            Initials = initials,

                            AvatarColor = isExamining
                                ? "#3B82F6"
                                : "#6366F1",

                            WaitingTime = q.Status,

                            IsUrgent = isExamining
                        });
                    }

                    LoadChartData();
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Lỗi kết nối cơ sở dữ liệu:\n{ex.Message}",
                        "Lỗi DB",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                });
            }
        }

        private void LoadChartData()
        {
            ChartLine.Clear();
            ChartBars.Clear();

            int[] lineValues = { 10, 15, 12, 25, 30, 28, 35, 42 };
            foreach (var val in lineValues)
            {
                ChartLine.Add(new DoctorDashboardData { Value = val });
            }

            int[] barValues = { 30, 50, 70, 45, 60 };
            foreach (var val in barValues)
            {
                ChartBars.Add(new DoctorDashboardData { Value = val });
            }
        }

        private void ExecuteLogoutCommand(object obj)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _refreshTimer?.Stop();
                UserSession.CurrentUser = null;
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var login = new LoginView();
                Application.Current.MainWindow = login;
                login.Show();
                foreach (var w in Application.Current.Windows.OfType<DoctorView>().ToList())
                {
                    w.Close();
                }
            }
        }
    }
}