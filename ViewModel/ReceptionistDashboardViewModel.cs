using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;
using TamAnh_EMR_System.View;
using TamAnh_EMR_System.View.Components;
using TamAnh_EMR_System.View.Receptionist;
using TamAnh_EMR_System.ViewModel.Receptionist;
using MediaColor = System.Windows.Media.Color;
using QuestColor = QuestPDF.Helpers.Colors;


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

        // ======================================================
        // PAGINATION / FILTER / SEARCH STATE
        // ======================================================

        private List<DashboardAppointment> _filteredAppointments;

        public ObservableCollection<int> PageNumbers { get; set; }

        private const int PageSize = 10;

        private CancellationTokenSource _searchCts;
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

        private bool _isTableLoading;

        public bool IsTableLoading
        {
            get => _isTableLoading;
            set
            {
                _isTableLoading = value;
                OnPropertyChanged(nameof(IsTableLoading));
            }
        }

        // =====================================================================
        // SEARCH PROPERTIES
        // Bound to the search box in the header
        // =====================================================================

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));

                _ = SearchAsync();
            }
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
        public ObservableCollection<ToastMessage> Toasts { get; set; }
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
            set
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));

                ApplyFilters();
            }
        }

        private string _selectedDoctor;
        /// <summary>Currently selected doctor filter (e.g., "Tất cả bác sĩ")</summary>
        public string SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                _selectedDoctor = value;
                OnPropertyChanged(nameof(SelectedDoctor));

                ApplyFilters();
            }
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
        public string PaginationText
        {
            get
            {
                if (TotalAppointments == 0)
                    return "Không có lịch hẹn";

                int start = ((CurrentPage - 1) * PageSize) + 1;

                int end = Math.Min(CurrentPage * PageSize, TotalAppointments);

                return $"Hiển thị {start} - {end} trong tổng số {TotalAppointments} lịch hẹn";
            }
        }
        private bool _isChartLoading;
        public bool IsChartLoading
        {
            get => _isChartLoading;
            set
            {
                _isChartLoading = value;
                OnPropertyChanged(nameof(IsChartLoading));
            }
        }
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
            PagedAppointments = new ObservableCollection<DashboardAppointment>();
            PageNumbers = new ObservableCollection<int>();
            Toasts = new ObservableCollection<ToastMessage>();
            Notifications = new ObservableCollection<NotificationItem>();
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

            LogoutCommand = new ViewModelCommand(ExecuteLogoutCommand);

            // Load static UI data
            LoadStaticData();

            // Set default view = Dashboard
            CurrentView = new DashboardContentControl
            {
                DataContext = this
            };

            // Load dashboard data from database
            IsTableLoading = true;
            _ = LoadDashboardFromDatabaseAsync();
            _ = LoadChartAsync();

            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());
            StartRealtimeRefresh();

        }

        private void ExecuteLogoutCommand(object obj)
        {
            var result = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginView loginView = new LoginView();
                loginView.Show();
                Application.Current.Windows
                    .OfType<Window>()
                    .SingleOrDefault(w => w is ReceptionistView)
                    ?.Close();
            }
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
                    _allAppointments = appointments;

                    _filteredAppointments = appointments;

                    TotalAppointments = appointments.Count;

                    TotalPages = Math.Max(
                        1,
                        (int)Math.Ceiling((double)TotalAppointments / PageSize)
                    );

                    if (!_isAutoRefreshing)
                    {
                        CurrentPage = 1;
                    }
                    else
                    {
                        if (CurrentPage > TotalPages)
                        {
                            CurrentPage = TotalPages;
                        }

                        if (CurrentPage < 1)
                        {
                            CurrentPage = 1;
                        }
                    }

                    GeneratePagination();

                    UpdatePagedAppointments();
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
                        ValueColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                        SubTextColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                        ShowProgress = false
                    });

                    StatisticCards.Add(new StatisticCard
                    {
                        Title = "ĐANG CHỜ",
                        Value = waiting.ToString(),
                        SubText = "Hiện tại",
                        ValueColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                        SubTextColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                        ShowProgress = true,
                        ProgressValue = total > 0 ? (double)waiting / total : 0,
                        ProgressColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B"))
                    });

                    StatisticCards.Add(new StatisticCard
                    {
                        Title = "ĐÃ HOÀN THÀNH",
                        Value = completed.ToString(),
                        SubText = $"/ {total}",
                        ValueColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                        SubTextColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                        ShowProgress = true,
                        ProgressValue = total > 0 ? (double)completed / total : 0,
                        ProgressColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B"))
                    });

                    StatisticCards.Add(new StatisticCard
                    {
                        Title = "TỶ LỆ HỦY",
                        Value = cancelled.ToString(),
                        SubText = total > 0 ? $"{(double)cancelled / total * 100:F1}%" : "0%",
                        ValueColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                        SubTextColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                        ShowProgress = true,
                        ProgressValue = total > 0 ? (double)cancelled / total : 0,
                        ProgressColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B"))
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
        private async Task LoadChartAsync()
        {
            try
            {
                IsChartLoading = true;

                var data = await _appointmentRepo.GetTodayChartDataAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChartData.Clear();

                    foreach (var item in data)
                    {
                        ChartData.Add(item);
                    }
                });
            }
            finally
            {
                IsChartLoading = false;
                IsTableLoading = false;
            }
        }

        // =====================================================================
        // COMMAND IMPLEMENTATIONS
        // =====================================================================

        private void ExecuteAddAppointment(object parameter)
        {
            SelectedMenu = "Hẹn lịch mới";

            CurrentView = new CreateAppointmentWindow();
        }

        private async void ExecuteExportReport(object parameter)
        {
            try
            {
                if (PagedAppointments == null || PagedAppointments.Count == 0)
                {
                    ShowWarningToast(
                        "Không có dữ liệu",
                        "Không có lịch hẹn để xuất báo cáo"
                    );

                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Title = "Xuất báo cáo lịch khám",
                    FileName = $"BaoCaoLichKham_{DateTime.Now:yyyyMMdd_HHmmss}",
                    Filter =
                        "CSV File (*.csv)|*.csv|" +
                        "PDF File (*.pdf)|*.pdf"
                };

                if (dialog.ShowDialog() != true)
                    return;

                await Task.Run(() =>
                {
                    string extension =
                        Path.GetExtension(dialog.FileName).ToLower();

                    // =====================================================
                    // CSV EXPORT
                    // =====================================================

                    if (extension == ".csv")
                    {
                        var sb = new StringBuilder();

                        sb.AppendLine(
                            "Thời gian,Bệnh nhân,Bác sĩ,Dịch vụ,Trạng thái"
                        );

                        foreach (var item in PagedAppointments)
                        {
                            sb.AppendLine(
                                $"{item.Time}," +
                                $"{item.PatientName}," +
                                $"{item.DoctorName}," +
                                $"{item.Service}," +
                                $"{item.Status}"
                            );
                        }

                        File.WriteAllText(
                            dialog.FileName,
                            sb.ToString(),
                            new UTF8Encoding(true)
                        );
                    }

                    // =====================================================
                    // PDF EXPORT
                    // =====================================================

                    else if (extension == ".pdf")
                    {
                        QuestPDF.Settings.License =
                            LicenseType.Community;

                        Document.Create(container =>
                        {
                            container.Page(page =>
                            {
                                page.Margin(30);
                                page.DefaultTextStyle(x =>
                                    x.FontSize(12)
                                );
                                page.Header()
                                    .Text("BÁO CÁO LỊCH KHÁM")
                                    .FontSize(22)
                                    .Bold();

                                page.Content()
                                    .PaddingVertical(20)
                                    .Column(col =>
                                    {
                                        col.Item().Text(
                                            $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}"
                                        );

                                        col.Item().PaddingTop(15);

                                        col.Item().Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(1);
                                                columns.RelativeColumn(2);
                                                columns.RelativeColumn(2);
                                                columns.RelativeColumn(2);
                                                columns.RelativeColumn(1);
                                            });

                                            // HEADER
                                            table.Header(header =>
                                            {
                                                header.Cell()
                                                    .Background(QuestColor.Blue.Lighten3)
                                                    .Padding(5)
                                                    .Text("Giờ")
                                                    .Bold();
                                                header.Cell()
                                                    .Background(QuestColor.Blue.Lighten3)
                                                    .Padding(5)
                                                    .Text("Bệnh nhân")
                                                    .Bold();
                                                header.Cell()
                                                    .Background(QuestColor.Blue.Lighten3)
                                                    .Padding(5)
                                                    .Text("Bác sĩ")
                                                    .Bold();
                                                header.Cell()
                                                    .Background(QuestColor.Blue.Lighten3)
                                                    .Padding(5)
                                                    .Text("Dịch vụ")
                                                    .Bold();
                                                header.Cell()
                                                    .Background(QuestColor.Blue.Lighten3)
                                                    .Padding(5)
                                                    .Text("Trạng thái")
                                                    .Bold();
                                                //header.Cell().Text("Bệnh nhân").Bold();
                                                //header.Cell().Text("Bác sĩ").Bold();
                                                //header.Cell().Text("Dịch vụ").Bold();
                                                //header.Cell().Text("Trạng thái").Bold();
                                            });

                                            // ROWS
                                            foreach (var item in PagedAppointments)
                                            {
                                                table.Cell().Text(item.Time);
                                                table.Cell().Text(item.PatientName);
                                                table.Cell().Text(item.DoctorName);
                                                table.Cell().Text(item.Service);
                                                table.Cell().Text(item.Status);
                                            }
                                        });
                                    });

                                page.Footer()
                                    .AlignCenter()
                                    .Text("TamAnh Hospital EMR System");
                            });
                        })
                        .GeneratePdf(dialog.FileName);
                    }
                });

                string fileType =
                Path.GetExtension(dialog.FileName)
                    .Replace(".", "")
                    .ToUpper();

                ShowSuccessToast(
                    "Xuất báo cáo thành công",
                    $"File {fileType} đã được tạo"
                );

                AddNotification(
                    "Xuất báo cáo",
                    "Đã xuất báo cáo lịch khám thành công",
                    "info"
                );
            }
            catch (Exception ex)
            {
                ShowErrorToast(
                    "Xuất báo cáo thất bại",
                    ex.Message
                );
            }
        }

        private void ExecuteNavigatePage(object parameter)
        {
            if (parameter == null)
                return;

            if (!int.TryParse(parameter.ToString(), out int page))
                return;

            if (page < 1 || page > TotalPages)
                return;

            CurrentPage = page;

            UpdatePagedAppointments();
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
                    CurrentView = new DashboardContentControl
                    {
                        DataContext = this
                    };
                    break;

                case "Đăng ký":
                    CurrentView = new RegisterPatientView();
                    break;

                case "Hẹn lịch mới":
                    CurrentView = new CreateAppointmentWindow();
                    break;
                // Future: uncomment when views are ready
                case "Tìm kiếm bệnh nhân":
                    CurrentView = new SearchPatientView();
                    break;
                case "Lịch hẹn":
                    CurrentView = new AppointmentManagementView
                    {
                        DataContext =
                            new AppointmentManagementViewModel()
                    };
                    break;

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
            Notifications.Add(new NotificationItem
            {
                Title = "Hệ thống khởi động",
                Description = "Dashboard đã được tải thành công",
                Type = "info",
                TimeText = DateTime.Now.ToString("HH:mm"),
                CreatedAt = DateTime.Now
            });
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
                ValueColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                SubTextColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                ShowProgress = false
            });

            StatisticCards.Add(new StatisticCard
            {
                Title = "ĐANG CHỜ",
                Value = "0",
                SubText = "—",
                ValueColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                SubTextColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                ShowProgress = false
            });

            StatisticCards.Add(new StatisticCard
            {
                Title = "ĐÃ HOÀN THÀNH",
                Value = "0",
                SubText = "/ 0",
                ValueColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                SubTextColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                ShowProgress = false
            });

            StatisticCards.Add(new StatisticCard
            {
                Title = "TỶ LỆ HỦY",
                Value = "0",
                SubText = "0%",
                ValueColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
                SubTextColor = new SolidColorBrush((MediaColor)ColorConverter.ConvertFromString("#1E293B")),
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
        private List<DashboardAppointment> _allAppointments;

        public ObservableCollection<DashboardAppointment> PagedAppointments { get; set; }
        // ======================================================
        // SEARCH
        // ======================================================

        private async Task SearchAsync()
        {
            try
            {
                _searchCts?.Cancel();

                _searchCts = new CancellationTokenSource();

                await Task.Delay(300, _searchCts.Token);

                ApplyFilters();
            }
            catch (TaskCanceledException)
            {
            }
        }

        // ======================================================
        // FILTER
        // ======================================================

        private void ApplyFilters()
        {
            if (_allAppointments == null)
                return;

            IEnumerable<DashboardAppointment> query = _allAppointments;

            // STATUS FILTER
            if (!string.IsNullOrWhiteSpace(SelectedStatus)
                && SelectedStatus != "Tất cả trạng thái")
            {
                query = query.Where(x => x.Status == SelectedStatus);
            }

            // DOCTOR FILTER
            if (!string.IsNullOrWhiteSpace(SelectedDoctor)
                && SelectedDoctor != "Tất cả bác sĩ")
            {
                query = query.Where(x => x.DoctorName == SelectedDoctor);
            }

            // SEARCH
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.Trim();

                query = query.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.PatientName)
                        && x.PatientName.Contains(keyword, StringComparison.OrdinalIgnoreCase))

                    ||

                    (!string.IsNullOrWhiteSpace(x.DoctorName)
                        && x.DoctorName.Contains(keyword, StringComparison.OrdinalIgnoreCase))

                    ||

                    (!string.IsNullOrWhiteSpace(x.Service)
                        && x.Service.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                );
            }

            _filteredAppointments = query.ToList();

            TotalAppointments = _filteredAppointments.Count;

            TotalPages = Math.Max(
                1,
                (int)Math.Ceiling((double)TotalAppointments / PageSize)
            );

            if (!_isAutoRefreshing)
            {
                CurrentPage = 1;
            }
            else
            {
                if (CurrentPage > TotalPages)
                {
                    CurrentPage = TotalPages;
                }
            }

            GeneratePagination();

            UpdatePagedAppointments();
        }

        // ======================================================
        // UPDATE PAGED DATA
        // ======================================================

        private void UpdatePagedAppointments()
        {
            PagedAppointments.Clear();

            if (_filteredAppointments == null)
                return;

            var items = _filteredAppointments
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            foreach (var item in items)
            {
                PagedAppointments.Add(item);
            }

            OnPropertyChanged(nameof(PaginationText));
        }

        // ======================================================
        // PAGE BUTTONS
        // ======================================================

        private void GeneratePagination()
        {
            PageNumbers.Clear();

            for (int i = 1; i <= TotalPages; i++)
            {
                PageNumbers.Add(i);
            }
        }
        public ICommand ResetFiltersCommand { get; }

        public ICommand LogoutCommand { get; }

        private void ResetFilters()
        {
            SelectedStatus = "Tất cả trạng thái";
            SelectedDoctor = "Tất cả bác sĩ";
            SearchText = "";

            ApplyFilters();
        }

        private DispatcherTimer _refreshTimer;

        private void StartRealtimeRefresh()
        {
            _refreshTimer = new DispatcherTimer();

            _refreshTimer.Interval = TimeSpan.FromSeconds(5);

            _refreshTimer.Tick += RefreshTimer_Tick;

            _refreshTimer.Start();
        }

        private async void ShowToast(
            string title,
            string message,
            string type = "success")
        {
            Brush bg = Brushes.Green;
            Brush border = Brushes.DarkGreen;

            switch (type)
            {
                case "error":
                    bg = Brushes.IndianRed;
                    border = Brushes.DarkRed;
                    break;

                case "warning":
                    bg = Brushes.DarkOrange;
                    border = Brushes.OrangeRed;
                    break;

                case "info":
                    bg = Brushes.DodgerBlue;
                    border = Brushes.RoyalBlue;
                    break;
            }

            var toast = new ToastMessage
            {
                Title = title,
                Message = message,
                Background = bg,
                BorderBrush = border
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                Toasts.Add(toast);
            });

            await Task.Delay(3000);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Toasts.Remove(toast);
            });
        }
        public void ShowSuccessToast(string title, string message)
        {
            ShowToast(title, message, "success");
        }

        public void ShowErrorToast(string title, string message)
        {
            ShowToast(title, message, "error");
        }

        public void ShowInfoToast(string title, string message)
        {
            ShowToast(title, message, "info");
        }
        public void ShowWarningToast(string title, string message)
        {
            ShowToast(title, message, "warning");
        }
        public ObservableCollection<NotificationItem> Notifications
        {
            get;
            set;
        }
        public void AddNotification(
            string title,
            string description,
            string type = "info")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Notifications.Insert(0, new NotificationItem
                {
                    Title = title,
                    Description = description,
                    Type = type,
                    TimeText = DateTime.Now.ToString("HH:mm"),
                    CreatedAt = DateTime.Now
                });

                NotificationCount = Notifications.Count;

                NewNotificationCount = Notifications.Count;

                if (Notifications.Count > 20)
                {
                    Notifications.RemoveAt(Notifications.Count - 1);
                }
            });
        }

        private bool _isRefreshing;
        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_isRefreshing)
                return;

            try
            {
                _isRefreshing = true;

                _isAutoRefreshing = true;

                await LoadDashboardFromDatabaseAsync();

                await LoadChartAsync();

                _isAutoRefreshing = false;
            }
            finally
            {
                _isRefreshing = false;
            }
        }
        private bool _isAutoRefreshing;
    }
}
