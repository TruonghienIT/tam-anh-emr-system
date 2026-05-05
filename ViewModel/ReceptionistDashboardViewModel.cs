using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.ViewModel
{
    /// <summary>
    /// ViewModel for the Receptionist Dashboard ("Tổng quan Lịch khám").
    /// 
    /// This ViewModel is the single source of truth for ALL data displayed on the dashboard.
    /// It provides:
    ///   - User info (name, role area)
    ///   - Statistics cards (4 KPI metrics)
    ///   - Appointment list (table data)
    ///   - Notifications list
    ///   - Chart data (hourly appointment density)
    ///   - Filter state (status, doctor)
    ///   - Pagination state
    ///   - Commands for all user actions
    /// 
    /// ObservableCollection is used for all list properties because it automatically
    /// notifies the UI when items are added/removed, keeping the view in sync.
    /// 
    /// All Commands use RelayCommand (ICommand) so buttons bind directly to ViewModel
    /// methods without any code-behind logic.
    /// 
    /// FUTURE: Replace LoadSampleData() with API calls to Laravel/Spring Boot backend.
    /// The ViewModel interface remains the same — only the data source changes.
    /// </summary>
    public class ReceptionistDashboardViewModel : ViewModelBase
    {
        // =====================================================================
        // USER INFO PROPERTIES
        // These display the logged-in receptionist's identity in the sidebar and header
        // =====================================================================

        private string _userDisplayName;
        /// <summary>User name shown in the header (e.g., "Lễ tân")</summary>
        public string UserDisplayName
        {
            get => _userDisplayName;
            set { _userDisplayName = value; OnPropertyChanged(nameof(UserDisplayName)); }
        }

        private string _userArea;
        /// <summary>Area label in sidebar (e.g., "Khu vực Lễ tân")</summary>
        public string UserArea
        {
            get => _userArea;
            set { _userArea = value; OnPropertyChanged(nameof(UserArea)); }
        }

        private string _userSubtitle;
        /// <summary>Role subtitle in sidebar (e.g., "Quản lý tiếp đón")</summary>
        public string UserSubtitle
        {
            get => _userSubtitle;
            set { _userSubtitle = value; OnPropertyChanged(nameof(UserSubtitle)); }
        }

        // =====================================================================
        // SEARCH PROPERTIES
        // Bound to the search box in the header
        // =====================================================================

        private string _searchText;
        /// <summary>Text entered in the header search box</summary>
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        // =====================================================================
        // DASHBOARD TITLE & SUBTITLE
        // Main heading area below the header
        // =====================================================================

        private string _dashboardTitle;
        public string DashboardTitle
        {
            get => _dashboardTitle;
            set { _dashboardTitle = value; OnPropertyChanged(nameof(DashboardTitle)); }
        }

        private string _dashboardSubtitle;
        public string DashboardSubtitle
        {
            get => _dashboardSubtitle;
            set { _dashboardSubtitle = value; OnPropertyChanged(nameof(DashboardSubtitle)); }
        }

        // =====================================================================
        // NOTIFICATION COUNT
        // Badge number on the notification bell icon in header
        // =====================================================================

        private int _notificationCount;
        public int NotificationCount
        {
            get => _notificationCount;
            set { _notificationCount = value; OnPropertyChanged(nameof(NotificationCount)); }
        }

        private int _newNotificationCount;
        /// <summary>Count shown in the "Tin mới" badge in notifications panel</summary>
        public int NewNotificationCount
        {
            get => _newNotificationCount;
            set { _newNotificationCount = value; OnPropertyChanged(nameof(NewNotificationCount)); }
        }

        // =====================================================================
        // STATISTICS CARDS
        // The 4 KPI cards at the top of the dashboard
        // ObservableCollection auto-notifies UI on add/remove
        // =====================================================================

        /// <summary>Collection of 4 statistic cards bound to ItemsControl</summary>
        public ObservableCollection<StatisticCard> StatisticCards { get; set; }

        // =====================================================================
        // APPOINTMENT LIST
        // Main table data bound to ItemsControl with DataTemplate
        // =====================================================================

        /// <summary>Collection of appointment rows for the dashboard table</summary>
        public ObservableCollection<DashboardAppointment> Appointments { get; set; }

        // =====================================================================
        // NOTIFICATIONS
        // Right-side panel showing recent alerts
        // =====================================================================

        /// <summary>Collection of recent notification items</summary>
        public ObservableCollection<Notification> Notifications { get; set; }

        // =====================================================================
        // CHART DATA
        // Bar chart showing hourly appointment density
        // =====================================================================

        /// <summary>Data points for the 24h appointment density chart</summary>
        public ObservableCollection<ChartDataPoint> ChartData { get; set; }

        // =====================================================================
        // FILTER PROPERTIES
        // Bound to the filter dropdowns above the appointment table
        // Prepared for future filtering logic
        // =====================================================================

        private string _selectedStatus;
        /// <summary>Currently selected status filter (e.g., "Tất cả trạng thái")</summary>
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(nameof(SelectedStatus)); }
        }

        private string _selectedDoctor;
        /// <summary>Currently selected doctor filter (e.g., "Tất cả bác sĩ")</summary>
        public string SelectedDoctor
        {
            get => _selectedDoctor;
            set { _selectedDoctor = value; OnPropertyChanged(nameof(SelectedDoctor)); }
        }

        /// <summary>Available status options for the filter dropdown</summary>
        public ObservableCollection<string> StatusOptions { get; set; }

        /// <summary>Available doctor options for the filter dropdown</summary>
        public ObservableCollection<string> DoctorOptions { get; set; }

        // =====================================================================
        // PAGINATION PROPERTIES
        // Controls the page navigation below the appointment table
        // =====================================================================

        private int _currentPage;
        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(nameof(CurrentPage)); OnPropertyChanged(nameof(PaginationText)); }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set { _totalPages = value; OnPropertyChanged(nameof(TotalPages)); }
        }

        private int _totalAppointments;
        public int TotalAppointments
        {
            get => _totalAppointments;
            set { _totalAppointments = value; OnPropertyChanged(nameof(TotalAppointments)); OnPropertyChanged(nameof(PaginationText)); }
        }

        /// <summary>Formatted pagination text (e.g., "Hiển thị 1 - 10 trong tổng số 42 lịch hẹn")</summary>
        public string PaginationText => $"Hiển thị 1 - 10 trong tổng số {TotalAppointments} lịch hẹn";

        // =====================================================================
        // SIDEBAR MENU ITEMS
        // Bound to ItemsControl in sidebar for dynamic menu rendering
        // =====================================================================

        /// <summary>Collection of sidebar navigation items</summary>
        public ObservableCollection<SidebarMenuItem> MenuItems { get; set; }

        // =====================================================================
        // COMMANDS
        // All user actions are bound to ICommand properties.
        // This keeps code-behind completely free of business logic.
        // =====================================================================

        /// <summary>Command for the "+ Hẹn lịch mới" button</summary>
        public ICommand AddAppointmentCommand { get; }

        /// <summary>Command for the "Xuất báo cáo" button</summary>
        public ICommand ExportReportCommand { get; }

        /// <summary>Command for page navigation buttons (parameter = page number)</summary>
        public ICommand NavigatePageCommand { get; }

        /// <summary>Command for the search action</summary>
        public ICommand SearchCommand { get; }

        /// <summary>Command for sidebar menu item clicks (parameter = menu item title)</summary>
        public ICommand MenuNavigateCommand { get; }

        /// <summary>Command for "Xem tất cả thông báo" link</summary>
        public ICommand ViewAllNotificationsCommand { get; }

        // =====================================================================
        // CONSTRUCTOR
        // Initializes all collections, loads sample data, and wires up commands
        // =====================================================================

        public ReceptionistDashboardViewModel()
        {
            // Initialize all ObservableCollections
            StatisticCards = new ObservableCollection<StatisticCard>();
            Appointments = new ObservableCollection<DashboardAppointment>();
            Notifications = new ObservableCollection<Notification>();
            ChartData = new ObservableCollection<ChartDataPoint>();
            MenuItems = new ObservableCollection<SidebarMenuItem>();
            StatusOptions = new ObservableCollection<string>();
            DoctorOptions = new ObservableCollection<string>();

            // Wire up commands using RelayCommand
            AddAppointmentCommand = new RelayCommand(ExecuteAddAppointment);
            ExportReportCommand = new RelayCommand(ExecuteExportReport);
            NavigatePageCommand = new RelayCommand(ExecuteNavigatePage);
            SearchCommand = new RelayCommand(ExecuteSearch);
            MenuNavigateCommand = new RelayCommand(ExecuteMenuNavigate);
            ViewAllNotificationsCommand = new RelayCommand(ExecuteViewAllNotifications);

            // Load demo data — replace with API calls in production
            LoadSampleData();
        }

        // =====================================================================
        // COMMAND IMPLEMENTATIONS
        // Placeholder methods ready for backend integration
        // =====================================================================

        private void ExecuteAddAppointment(object parameter)
        {
            // TODO: Open new appointment dialog/window
            System.Windows.MessageBox.Show("Mở form tạo lịch hẹn mới", "Hẹn lịch mới");
        }

        private void ExecuteExportReport(object parameter)
        {
            // TODO: Export appointment data to Excel/PDF
            System.Windows.MessageBox.Show("Xuất báo cáo lịch khám", "Xuất báo cáo");
        }

        private void ExecuteNavigatePage(object parameter)
        {
            // TODO: Load appointments for the selected page
            if (parameter is string pageStr && int.TryParse(pageStr, out int page))
            {
                CurrentPage = page;
            }
        }

        private void ExecuteSearch(object parameter)
        {
            // TODO: Filter appointments based on SearchText
        }

        private void ExecuteMenuNavigate(object parameter)
        {
            // TODO: Navigate to the selected menu section
            if (parameter is string menuTitle)
            {
                foreach (var item in MenuItems)
                {
                    item.IsSelected = item.Title == menuTitle;
                }
            }
        }

        private void ExecuteViewAllNotifications(object parameter)
        {
            // TODO: Navigate to full notifications view
            System.Windows.MessageBox.Show("Xem tất cả thông báo", "Thông báo");
        }

        // =====================================================================
        // SAMPLE DATA LOADER
        // Populates all collections with demo data matching the design screenshot.
        // In production, replace this with service/repository calls.
        // =====================================================================

        private void LoadSampleData()
        {
            // --- User Info ---
            UserDisplayName = "Lễ tân";
            UserArea = "Khu vực Lễ tân";
            UserSubtitle = "Quản lý tiếp đón";
            SearchText = "";
            NotificationCount = 3;
            NewNotificationCount = 3;

            // --- Dashboard Title ---
            DashboardTitle = "Tổng quan Lịch khám";
            DashboardSubtitle = "Chào buổi sáng! Đây là tóm tắt lịch trình phòng khám ngày hôm nay.";

            // --- Statistics Cards (4 KPI cards matching design) ---
            StatisticCards.Add(new StatisticCard
            {
                Title = "TỔNG LỊCH HẸN",
                Value = "42",
                SubText = "↑12%",
                ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                ShowProgress = false
            });

            StatisticCards.Add(new StatisticCard
            {
                Title = "ĐANG CHỜ",
                Value = "18",
                SubText = "Hiện tại",
                ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                ShowProgress = true,
                ProgressValue = 0.43,
                ProgressColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"))
            });

            StatisticCards.Add(new StatisticCard
            {
                Title = "ĐÃ HOÀN THÀNH",
                Value = "14",
                SubText = "/ 42",
                ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06B6D4")),
                SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                ShowProgress = true,
                ProgressValue = 0.33,
                ProgressColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06B6D4"))
            });

            StatisticCards.Add(new StatisticCard
            {
                Title = "TỶ LỆ HỦY",
                Value = "2",
                SubText = "4.7%",
                ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                ShowProgress = true,
                ProgressValue = 0.05,
                ProgressColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"))
            });

            // --- Appointments (matching design rows exactly) ---
            Appointments.Add(new DashboardAppointment
            {
                Time = "08:30",
                Date = "THỨ 2, 24/05",
                PatientName = "Nguyễn Thành Long",
                PatientInitials = "NL",
                GenderAge = "Nam, 32 tuổi",
                DoctorName = "BS. Trần Đức Anh",
                Department = "Khoa Nội",
                Service = "Khám tổng quát",
                Status = "Đang khám",
                AvatarColor = "#6366F1"
            });

            Appointments.Add(new DashboardAppointment
            {
                Time = "09:00",
                Date = "THỨ 2, 24/05",
                PatientName = "Phạm Minh Hương",
                PatientInitials = "PH",
                GenderAge = "Nữ, 28 tuổi",
                DoctorName = "BS. Lê Thu Thủy",
                Department = "Khoa Sản",
                Service = "Siêu âm thai",
                Status = "Đang chờ",
                AvatarColor = "#EC4899"
            });

            Appointments.Add(new DashboardAppointment
            {
                Time = "09:15",
                Date = "THỨ 2, 24/05",
                PatientName = "Vũ Anh Duy",
                PatientInitials = "VD",
                GenderAge = "Nam, 45 tuổi",
                DoctorName = "BS. Nguyễn Văn A",
                Department = "Khoa Tai Mũi Họng",
                Service = "Nội soi TMH",
                Status = "Hoàn thành",
                AvatarColor = "#10B981"
            });

            Appointments.Add(new DashboardAppointment
            {
                Time = "09:30",
                Date = "THỨ 2, 24/05",
                PatientName = "Trần Quốc Quân",
                PatientInitials = "TQ",
                GenderAge = "Nam, 19 tuổi",
                DoctorName = "BS. Phạm Văn C",
                Department = "Khoa RHM",
                Service = "Lấy cao răng",
                Status = "Đã hủy",
                AvatarColor = "#F59E0B"
            });

            // --- Filter Options ---
            StatusOptions.Add("Tất cả trạng thái");
            StatusOptions.Add("Đang khám");
            StatusOptions.Add("Đang chờ");
            StatusOptions.Add("Hoàn thành");
            StatusOptions.Add("Đã hủy");
            SelectedStatus = "Tất cả trạng thái";

            DoctorOptions.Add("Tất cả bác sĩ");
            DoctorOptions.Add("BS. Trần Đức Anh");
            DoctorOptions.Add("BS. Lê Thu Thủy");
            DoctorOptions.Add("BS. Nguyễn Văn A");
            DoctorOptions.Add("BS. Phạm Văn C");
            SelectedDoctor = "Tất cả bác sĩ";

            // --- Pagination ---
            CurrentPage = 1;
            TotalPages = 3;
            TotalAppointments = 42;

            // --- Sidebar Menu ---
            MenuItems.Add(new SidebarMenuItem { Title = "Tổng quan", Icon = FontAwesome.Sharp.IconChar.ChartLine, IsSelected = true });
            MenuItems.Add(new SidebarMenuItem { Title = "Tìm kiếm bệnh nhân", Icon = FontAwesome.Sharp.IconChar.UserGroup, IsSelected = false });
            MenuItems.Add(new SidebarMenuItem { Title = "Đăng ký", Icon = FontAwesome.Sharp.IconChar.UserPlus, IsSelected = false });
            MenuItems.Add(new SidebarMenuItem { Title = "Lịch hẹn", Icon = FontAwesome.Sharp.IconChar.CalendarCheck, IsSelected = false });

            // --- Notifications (matching design) ---
            Notifications.Add(new Notification
            {
                Title = "Bệnh nhân mới đăng ký khám",
                Description = "Lê Hồng Phúc (24 tuổi) - 10:15 sáng nay",
                TimeDisplay = "10:15 sáng nay",
                Time = DateTime.Now.AddHours(-1),
                DotColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"))
            });

            Notifications.Add(new Notification
            {
                Title = "Bác sĩ Trần Đức Anh yêu cầu hỗ trợ",
                Description = "Vật tưPhòng khám 03 cần bổ sung",
                TimeDisplay = "9:30 sáng nay",
                Time = DateTime.Now.AddHours(-2),
                DotColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"))
            });

            Notifications.Add(new Notification
            {
                Title = "Lịch hẹn bị hủy bởi khách hàng",
                Description = "Ngô Văn Tú - Lịch 14:00 chiều",
                TimeDisplay = "8:00 sáng nay",
                Time = DateTime.Now.AddHours(-3),
                DotColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"))
            });

            // --- Chart Data (hourly appointment density, 08H-15H) ---
            double maxVal = 18;
            ChartData.Add(new ChartDataPoint { Label = "08H", Value1 = 8, Value2 = 5, MaxValue = maxVal });
            ChartData.Add(new ChartDataPoint { Label = "09H", Value1 = 12, Value2 = 7, MaxValue = maxVal });
            ChartData.Add(new ChartDataPoint { Label = "10H", Value1 = 18, Value2 = 10, MaxValue = maxVal });
            ChartData.Add(new ChartDataPoint { Label = "11H", Value1 = 15, Value2 = 8, MaxValue = maxVal });
            ChartData.Add(new ChartDataPoint { Label = "12H", Value1 = 5, Value2 = 3, MaxValue = maxVal });
            ChartData.Add(new ChartDataPoint { Label = "13H", Value1 = 14, Value2 = 9, MaxValue = maxVal });
            ChartData.Add(new ChartDataPoint { Label = "14H", Value1 = 16, Value2 = 11, MaxValue = maxVal });
            ChartData.Add(new ChartDataPoint { Label = "15H", Value1 = 10, Value2 = 6, MaxValue = maxVal });
        }
    }
}
