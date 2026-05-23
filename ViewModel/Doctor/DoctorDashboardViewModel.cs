using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

        private PointCollection _patientTrendLinePoints;
        public PointCollection PatientTrendLinePoints
        {
            get => _patientTrendLinePoints;
            set { _patientTrendLinePoints = value; OnPropertyChanged(nameof(PatientTrendLinePoints)); }
        }

        private PointCollection _patientTrendFillPoints;
        public PointCollection PatientTrendFillPoints
        {
            get => _patientTrendFillPoints;
            set { _patientTrendFillPoints = value; OnPropertyChanged(nameof(PatientTrendFillPoints)); }
        }

        private string _trendPercentage;
        public string TrendPercentage
        {
            get => _trendPercentage;
            set { _trendPercentage = value; OnPropertyChanged(nameof(TrendPercentage)); }
        }

        private string _trendColor;
        public string TrendColor
        {
            get => _trendColor;
            set { _trendColor = value; OnPropertyChanged(nameof(TrendColor)); }
        }

        private string _trendIcon;
        public string TrendIcon
        {
            get => _trendIcon;
            set { _trendIcon = value; OnPropertyChanged(nameof(TrendIcon)); }
        }

        private int _maxPatientCount;
        public int MaxPatientCount
        {
            get => _maxPatientCount;
            set { _maxPatientCount = value; OnPropertyChanged(nameof(MaxPatientCount)); }
        }

        private int _halfMaxPatientCount;
        public int HalfMaxPatientCount
        {
            get => _halfMaxPatientCount;
            set { _halfMaxPatientCount = value; OnPropertyChanged(nameof(HalfMaxPatientCount)); }
        }

        private int _maxDiseaseCount;
        public int MaxDiseaseCount
        {
            get => _maxDiseaseCount;
            set { _maxDiseaseCount = value; OnPropertyChanged(nameof(MaxDiseaseCount)); }
        }

        private int _halfMaxDiseaseCount;
        public int HalfMaxDiseaseCount
        {
            get => _halfMaxDiseaseCount;
            set { _halfMaxDiseaseCount = value; OnPropertyChanged(nameof(HalfMaxDiseaseCount)); }
        }

        private ObservableCollection<DoctorDashboardData> _weeklyPatientBars;
        public ObservableCollection<DoctorDashboardData> WeeklyPatientBars
        {
            get => _weeklyPatientBars;
            set { _weeklyPatientBars = value; OnPropertyChanged(nameof(WeeklyPatientBars)); }
        }
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
                Interval = TimeSpan.FromSeconds(10)
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

            // Lấy dữ liệu appointments từ database
            _ = LoadChartDataFromDatabaseAsync();
        }

        private async Task LoadChartDataFromDatabaseAsync()
        {
            try
            {
                // Lấy dữ liệu từ Database
                var diseaseData = await _repository.GetDiseaseStatisticsAsync();
                var trendData = await _repository.GetPatientTrendLast7DaysAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // =======================================================
                    // 1. XỬ LÝ BIỂU ĐỒ BỆNH NHÂN 7 NGÀY QUA
                    // =======================================================
                    int todayCount = trendData[6];
                    int yesterdayCount = trendData[5];

                    // Tính toán % tăng giảm
                    if (yesterdayCount == 0)
                    {
                        TrendPercentage = todayCount > 0 ? "+100%" : "0%";
                        TrendColor = todayCount > 0 ? "#10B981" : "#9CA3AF";
                        TrendIcon = todayCount > 0 ? "ArrowTrendUp" : "Minus";
                    }
                    else
                    {
                        double percent = ((double)(todayCount - yesterdayCount) / yesterdayCount) * 100;
                        TrendPercentage = (percent > 0 ? "+" : "") + percent.ToString("F0") + "%";
                        TrendColor = percent >= 0 ? "#10B981" : "#EF4444";
                        TrendIcon = percent >= 0 ? "ArrowTrendUp" : "ArrowTrendDown";
                    }

                    // Tạo dữ liệu cho Biểu đồ cột mini (7 ngày)
                    double maxPatientHeight = 150;
                    int actualMaxPatient = trendData.Max();
                    int maxPatientData = actualMaxPatient % 2 != 0 ? actualMaxPatient + 1 : actualMaxPatient;
                    if (maxPatientData < 4) maxPatientData = 4;

                    MaxPatientCount = maxPatientData;
                    HalfMaxPatientCount = maxPatientData / 2;

                    var bars = new ObservableCollection<DoctorDashboardData>();
                    for (int i = 0; i < 7; i++)
                    {
                        double h = ((double)trendData[i] / maxPatientData) * maxPatientHeight;
                        if (h < 4) h = 4;

                        string dateLabel = (i == 6) ? "Hôm nay" : DateTime.Now.AddDays(i - 6).ToString("dd/MM");

                        bars.Add(new DoctorDashboardData
                        {
                            Value = trendData[i],
                            BarHeight = h,
                            Label = dateLabel,
                            Tooltip = $"{dateLabel}: {trendData[i]} bệnh nhân"
                        });
                    }
                    WeeklyPatientBars = bars;
                });

                // =======================================================
                // 2. XỬ LÝ BIỂU ĐỒ THỐNG KÊ BỆNH
                // =======================================================
                if (diseaseData == null || diseaseData.Count == 0)
                    return;

                // Tính toán thông số cho trục Y của Thống kê bệnh
                int actualMaxDisease = diseaseData.Values.Max();
                int maxDiseaseData = actualMaxDisease % 2 != 0 ? actualMaxDisease + 1 : actualMaxDisease;
                if (maxDiseaseData < 4) maxDiseaseData = 4;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Cập nhật trục Y ra UI
                    MaxDiseaseCount = maxDiseaseData;
                    HalfMaxDiseaseCount = maxDiseaseData / 2;

                    ChartBars.Clear();
                    foreach (var kvp in diseaseData)
                    {
                        var dataPoint = new DoctorDashboardData();
                        dataPoint.Label = kvp.Key;
                        dataPoint.Value = kvp.Value;

                        // Tính toán chiều cao cột (MaxHeight trong UI là 150)
                        double h = ((double)kvp.Value / maxDiseaseData) * 150;
                        if (h < 4) h = 4;
                        dataPoint.BarHeight = h;

                        dataPoint.Tooltip = $"{kvp.Key}: {kvp.Value} chẩn đoán";

                        ChartBars.Add(dataPoint);
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Lỗi khi tải dữ liệu chart:\n{ex.Message}",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                });
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