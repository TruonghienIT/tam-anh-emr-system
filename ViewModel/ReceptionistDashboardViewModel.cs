using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;
using TamAnh_EMR_System.View;
using TamAnh_EMR_System.View.Components;
using TamAnh_EMR_System.View.Receptionist;

namespace TamAnh_EMR_System.ViewModel
{
    /// <summary>
    /// ViewModel for the Receptionist Dashboard ("Tổng quan Lịch khám").
    /// 
    /// This ViewModel is the single source of truth for ALL data displayed on the dashboard.
    /// It provides:
    ///   - User info (name, role area)
    ///   - Statistics cards (4 KPI metrics) — loaded from DB
    ///   - Appointment list (table data) — loaded from DB
    ///   - Notifications list
    ///   - Chart data (hourly appointment density)
    ///   - Filter state (status, doctor)
    ///   - Pagination state
    ///   - Commands for all user actions
    /// 
    /// KEY CHANGES FROM ORIGINAL:
    /// - Dashboard appointments now load from SQL Server via AppointmentRepository
    /// - Statistics cards compute from real DB data
    /// - "Hẹn lịch mới" button opens CreateAppointmentWindow popup
    /// - After popup closes with success, dashboard reloads from DB
    /// </summary>
    public class ReceptionistDashboardViewModel : ViewModelBase
    {
        // =====================================================================
        // REPOSITORY
        // =====================================================================

        private readonly AppointmentRepository _appointmentRepo;

        // =====================================================================
        // CURRENT VIEW (for dynamic view switching)
        // ContentControl in ReceptionistView.xaml binds to this property.
        // Changing CurrentView swaps the entire content area.
        // =====================================================================

        private UserControl _currentView;
        /// <summary>
        /// The currently displayed UserControl in the main content area.
        /// Default: DashboardContentControl (tổng quan).
        /// Changed by ExecuteMenuNavigate when sidebar items are clicked.
        /// </summary>
        public UserControl CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        // =====================================================================
        // USER INFO PROPERTIES
        // These display the logged-in receptionist's identity in the sidebar and header
        // =====================================================================

        private string _selectedMenu = "Tổng quan";
        /// <summary>
        /// Currently active sidebar menu item title. Drives sidebar highlighting.
        /// </summary>
        public string SelectedMenu
        {
            get => _selectedMenu;
            set { _selectedMenu = value; OnPropertyChanged(nameof(SelectedMenu)); }
        }

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
        public string PaginationText => $"Hiển thị 1 - {Math.Min(10, TotalAppointments)} trong tổng số {TotalAppointments} lịch hẹn";

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
        // Initializes all collections, loads data from DB, and wires up commands
        // =====================================================================

        public ReceptionistDashboardViewModel()
        {
            // Initialize repository
            _appointmentRepo = new AppointmentRepository();

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

            // Load static UI data
            LoadStaticData();

            // Set default view = Dashboard
            CurrentView = new DashboardContentControl();

            // Load dashboard data from database
            _ = LoadDashboardFromDatabaseAsync();
        }

        // =====================================================================
        // DATABASE LOADING
        // Loads appointments and statistics from SQL Server
        // =====================================================================

        /// <summary>
        /// Loads all dashboard data from the database.
        /// Called on startup and after creating a new appointment.
        /// </summary>
        private async Task LoadDashboardFromDatabaseAsync()
        {
            try
            {
                // Load appointments from DB
                var appointments = await _appointmentRepo.GetDashboardAppointmentsAsync(DateTime.Today);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Appointments.Clear();
                    foreach (var apt in appointments)
                        Appointments.Add(apt);

                    TotalAppointments = appointments.Count;
                    TotalPages = Math.Max(1, (int)Math.Ceiling(TotalAppointments / 10.0));
                    CurrentPage = 1;
                });

                // Load statistics from DB
                var stats = await _appointmentRepo.GetTodayStatisticsAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatisticCards.Clear();

                    int total = stats["total"];
                    int waiting = stats["waiting"];
                    int completed = stats["completed"];
                    int cancelled = stats["cancelled"];

                    StatisticCards.Add(new StatisticCard
                    {
                        Title = "TỔNG LỊCH HẸN",
                        Value = total.ToString(),
                        SubText = "Hôm nay",
                        ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                        SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                        ShowProgress = false
                    });

                    StatisticCards.Add(new StatisticCard
                    {
                        Title = "ĐANG CHỜ",
                        Value = waiting.ToString(),
                        SubText = "Hiện tại",
                        ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                        SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                        ShowProgress = true,
                        ProgressValue = total > 0 ? (double)waiting / total : 0,
                        ProgressColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"))
                    });

                    StatisticCards.Add(new StatisticCard
                    {
                        Title = "ĐÃ HOÀN THÀNH",
                        Value = completed.ToString(),
                        SubText = $"/ {total}",
                        ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06B6D4")),
                        SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                        ShowProgress = true,
                        ProgressValue = total > 0 ? (double)completed / total : 0,
                        ProgressColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06B6D4"))
                    });

                    StatisticCards.Add(new StatisticCard
                    {
                        Title = "TỶ LỆ HỦY",
                        Value = cancelled.ToString(),
                        SubText = total > 0 ? $"{(double)cancelled / total * 100:F1}%" : "0%",
                        ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                        SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                        ShowProgress = true,
                        ProgressValue = total > 0 ? (double)cancelled / total : 0,
                        ProgressColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"))
                    });
                });

                // Extract unique doctors for filter dropdown
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DoctorOptions.Clear();
                    DoctorOptions.Add("Tất cả bác sĩ");
                    var doctorNames = new System.Collections.Generic.HashSet<string>();
                    foreach (var apt in appointments)
                    {
                        if (!string.IsNullOrEmpty(apt.DoctorName) && doctorNames.Add(apt.DoctorName))
                            DoctorOptions.Add(apt.DoctorName);
                    }
                    SelectedDoctor = "Tất cả bác sĩ";
                });
            }
            catch (Exception ex)
            {
                // If DB is unavailable, show friendly message and load sample data as fallback
                System.Diagnostics.Debug.WriteLine($"DB load failed: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadFallbackSampleData();
                });
            }
        }

        // =====================================================================
        // COMMAND IMPLEMENTATIONS
        // =====================================================================

        private void ExecuteAddAppointment(object parameter)
        {
            // Open CreateAppointmentWindow as a modal dialog
            var window = new CreateAppointmentWindow();
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();

            // If appointment was created successfully, reload dashboard from DB
            if (result == true)
            {
                _ = LoadDashboardFromDatabaseAsync();
            }
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
            if (parameter is not string menuTitle) return;

            // Update SelectedMenu for sidebar highlighting via DataTrigger
            SelectedMenu = menuTitle;

            // Update sidebar selection state
            foreach (var item in MenuItems)
            {
                item.IsSelected = item.Title == menuTitle;
            }

            // Switch the content area view based on menu title
            switch (menuTitle)
            {
                case "Tổng quan":
                    CurrentView = new DashboardContentControl();
                    break;

                case "Đăng ký":
                    CurrentView = new RegisterPatientView();
                    break;

                // Future: uncomment when views are ready
                // case "Tìm kiếm bệnh nhân":
                //     CurrentView = new SearchPatientView();
                //     break;
                // case "Lịch hẹn":
                //     CurrentView = new CreateAppointmentView();
                //     break;

                default:
                    // Unknown menu — stay on current view
                    break;
            }
        }

        private void ExecuteViewAllNotifications(object parameter)
        {
            // TODO: Navigate to full notifications view
            System.Windows.MessageBox.Show("Xem tất cả thông báo", "Thông báo");
        }

        // =====================================================================
        // STATIC DATA LOADER
        // Loads data that doesn't come from the database
        // =====================================================================

        private void LoadStaticData()
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

            // --- Filter Options ---
            StatusOptions.Add("Tất cả trạng thái");
            StatusOptions.Add("Đang khám");
            StatusOptions.Add("Đang chờ");
            StatusOptions.Add("Hoàn thành");
            StatusOptions.Add("Đã hủy");
            SelectedStatus = "Tất cả trạng thái";

            // --- Sidebar Menu ---
            MenuItems.Add(new SidebarMenuItem { Title = "Tổng quan", Icon = FontAwesome.Sharp.IconChar.ChartLine, IsSelected = true });
            MenuItems.Add(new SidebarMenuItem { Title = "Tìm kiếm bệnh nhân", Icon = FontAwesome.Sharp.IconChar.UserGroup, IsSelected = false });
            MenuItems.Add(new SidebarMenuItem { Title = "Đăng ký", Icon = FontAwesome.Sharp.IconChar.UserPlus, IsSelected = false });
            MenuItems.Add(new SidebarMenuItem { Title = "Lịch hẹn", Icon = FontAwesome.Sharp.IconChar.CalendarCheck, IsSelected = false });

            // --- Notifications ---
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
                Description = "Vật tư Phòng khám 03 cần bổ sung",
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

        // =====================================================================
        // FALLBACK SAMPLE DATA
        // Used only when database is unavailable
        // =====================================================================

        private void LoadFallbackSampleData()
        {
            // --- Statistics Cards (fallback) ---
            StatisticCards.Clear();
            StatisticCards.Add(new StatisticCard
            {
                Title = "TỔNG LỊCH HẸN",
                Value = "0",
                SubText = "Chưa kết nối DB",
                ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                ShowProgress = false
            });

            StatisticCards.Add(new StatisticCard
            {
                Title = "ĐANG CHỜ",
                Value = "0",
                SubText = "—",
                ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                ShowProgress = false
            });

            StatisticCards.Add(new StatisticCard
            {
                Title = "ĐÃ HOÀN THÀNH",
                Value = "0",
                SubText = "/ 0",
                ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06B6D4")),
                SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                ShowProgress = false
            });

            StatisticCards.Add(new StatisticCard
            {
                Title = "TỶ LỆ HỦY",
                Value = "0",
                SubText = "0%",
                ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                SubTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                ShowProgress = false
            });

            // --- Appointments (empty) ---
            Appointments.Clear();
            TotalAppointments = 0;
            TotalPages = 1;
            CurrentPage = 1;

            // --- Doctor filter (empty) ---
            DoctorOptions.Clear();
            DoctorOptions.Add("Tất cả bác sĩ");
            SelectedDoctor = "Tất cả bác sĩ";
        }
    }
}
