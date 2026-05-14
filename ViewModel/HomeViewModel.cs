using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly HomeViewRepository repository;

        public HomeViewModel()
        {
            repository = new HomeViewRepository();

            CurrentDate = DateTime.Now.ToString("dddd, dd/MM/yyyy | HH:mm");

            ChartTypes = new ObservableCollection<string>
            {
                "Giới tính",
                "Độ tuổi",
                "Nhóm máu"
            };

            SelectedChartType = "Giới tính";

            LoadDashboardCommand = new ViewModelCommand(_ => LoadDashboard());

            LoadDashboard();
        }

        #region Dashboard Count

        private int _patientCount;
        public int PatientCount
        {
            get => _patientCount;
            set
            {
                _patientCount = value;
                OnPropertyChanged(nameof(PatientCount));
            }
        }

        private int _appointmentCount;

        public int AppointmentCount
        {
            get => _appointmentCount;
            set
            {
                _appointmentCount = value;
                OnPropertyChanged(nameof(AppointmentCount));
            }
        }

        private int _medicalRecordCount;

        public int MedicalRecordCount
        {
            get => _medicalRecordCount;
            set
            {
                _medicalRecordCount = value;
                OnPropertyChanged(nameof(MedicalRecordCount));
            }
        }

        private int _doctorCount;

        public int DoctorCount
        {
            get => _doctorCount;
            set
            {
                _doctorCount = value;
                OnPropertyChanged(nameof(DoctorCount));
            }
        }

        #endregion

        #region Dashboard Sub Text

        private string _patientGrowth;
        public string PatientGrowth
        {
            get => _patientGrowth;
            set
            {
                _patientGrowth = value;
                OnPropertyChanged(nameof(PatientGrowth));
            }
        }

        private string _appointmentPending;

        public string AppointmentPending
        {
            get => _appointmentPending;
            set
            {
                _appointmentPending = value;
                OnPropertyChanged(nameof(AppointmentPending));
            }
        }

        private string _medicalRecordToday;

        public string MedicalRecordToday
        {
            get => _medicalRecordToday;
            set
            {
                _medicalRecordToday = value;
                OnPropertyChanged(nameof(MedicalRecordToday));
            }
        }

        private string _doctorSpecializationInfo;

        public string DoctorSpecializationInfo
        {
            get => _doctorSpecializationInfo;
            set
            {
                _doctorSpecializationInfo = value;
                OnPropertyChanged(nameof(DoctorSpecializationInfo));
            }
        }



        #endregion

        #region Current Date

        private string _currentDate;

        public string CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged(nameof(CurrentDate));
            }
        }

        #endregion

        #region Appointment

        private ObservableCollection<Appointment> _recentAppointments;

        public ObservableCollection<Appointment> RecentAppointments
        {
            get => _recentAppointments;
            set
            {
                _recentAppointments = value;
                OnPropertyChanged(nameof(RecentAppointments));
            }
        }


        #endregion

        #region Notification
        private ObservableCollection<NotificationItem> _recentActivities;

        public ObservableCollection<NotificationItem> RecentActivities
        {
            get => _recentActivities;
            set
            {
                _recentActivities = value;
                OnPropertyChanged(nameof(RecentActivities));
            }
        }

        #endregion

        #region Chart

        private ObservableCollection<ChartDataPoint> _patientChart;

        public ObservableCollection<ChartDataPoint> PatientChart
        {
            get => _patientChart;
            set
            {
                _patientChart = value;
                OnPropertyChanged(nameof(PatientChart));
            }
        }

        private ObservableCollection<ChartBar> _patientBars;

        public ObservableCollection<ChartBar> PatientBars
        {
            get => _patientBars;
            set
            {
                _patientBars = value;
                OnPropertyChanged(nameof(PatientBars));
            }
        }

        public ObservableCollection<string> ChartTypes { get; set; }

        private string _selectedChartType;

        public string SelectedChartType
        {
            get => _selectedChartType;
            set
            {
                _selectedChartType = value;

                OnPropertyChanged(nameof(SelectedChartType));

                LoadChartByType();
            }
        }

        #endregion

        public ICommand LoadDashboardCommand { get; }

        private void LoadDashboard()
        {
            try
            {
                PatientCount = repository.GetPatientCount();

                AppointmentCount = repository.GetAppointmentCount();

                MedicalRecordCount = repository.GetMedicalRecordCount();

                DoctorCount = repository.GetDoctorCount();

                AppointmentPending =
                    $"{repository.GetPendingAppointmentCount()} đang chờ";

                MedicalRecordToday =
                    $"+{repository.GetTodayMedicalRecordCount()} hôm nay";

                DoctorSpecializationInfo =
                    $"{repository.GetSpecializationCount()} chuyên khoa";

                PatientGrowth = repository.GetPatientGrowth();

                RecentAppointments = repository.GetTodayAppointments();

                RecentActivities = repository.GetRecentActivities();
                LoadChartByType();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadChartByType()
        {
            if (repository == null)
                return;

            switch (SelectedChartType)
            {
                case "Giới tính":
                    PatientChart = repository.GetPatientChartByGender();
                    break;

                case "Độ tuổi":
                    PatientChart = repository.GetPatientChartByAge();
                    break;

                case "Nhóm máu":
                    PatientChart = repository.GetPatientChartByBloodType();
                    break;

                default:
                    PatientChart = repository.GetPatientChartByGender();
                    break;
            }

            BuildColumnChart();
        }

        private void BuildColumnChart()
        {
            var bars = new ObservableCollection<ChartBar>();

            double canvasWidth = 760;

            double barWidth = 50;

            double baseY = 200;

            double scale = 35;

            int count = PatientChart.Count;

            if (count == 0)
                return;

            double spacing = canvasWidth / count;

            double offset = (spacing - barWidth) / 2;

            for (int i = 0; i < count; i++)
            {
                var item = PatientChart[i];

                double height = item.Value * scale;

                bars.Add(new ChartBar
                {
                    X = i * spacing + offset,

                    Y = baseY - height,

                    Height = height,

                    Width = barWidth,

                    Value = (int)item.Value,

                    Label = item.Label
                });
            }

            PatientBars = bars;
        }
    }

    public class ChartBar
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Height { get; set; }

        public double Width { get; set; } = 50;

        public int Value { get; set; }

        public string Label { get; set; }
    }
}