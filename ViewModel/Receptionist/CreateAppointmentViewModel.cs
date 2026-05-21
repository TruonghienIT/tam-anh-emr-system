using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Helper;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Model.Doctor;
using TamAnh_EMR_System.Model.Receptionist;
using TamAnh_EMR_System.Repositories;
using TamAnh_EMR_System.Services;
using TamAnh_EMR_System.View.Receptionist;
using TamAnh_EMR_System.Services.Pdf;

namespace TamAnh_EMR_System.ViewModel.Receptionist
{
    /// <summary>
    /// ViewModel for "Tạo lịch hẹn mới" screen.
    /// 
    /// Manages:
    /// - Patient search from DB (PatientRepository.SearchAsync)
    /// - Doctor list from DB (DoctorPanelRepository.GetAllDoctors)
    /// - Calendar navigation and date selection
    /// - Time slot selection with doctor conflict checking
    /// - Form validation (name, phone, date, doctor conflict)
    /// - Appointment submission via AppointmentRegistrationService (transaction)
    /// - CloseAction callback to close the popup window
    /// 
    /// NO FAKE DATA — all data comes from SQL Server.
    /// </summary>
    public class CreateAppointmentViewModel : ViewModelBase
    {
        // =====================================================================
        // SERVICES & REPOSITORIES
        // =====================================================================

        private readonly AppointmentRegistrationService _registrationService;
        private readonly PatientRepository _patientRepo;
        private readonly AppointmentRepository _appointmentRepo;
        private readonly DoctorPanelRepository _doctorRepo;

        // =====================================================================
        // CLOSE CALLBACK
        // Set by the Window code-behind to allow ViewModel to close the popup
        // =====================================================================

        public Action<bool> CloseAction { get; set; }

        // =====================================================================
        // APPOINTMENT FORM DATA
        // =====================================================================

        private AppointmentForm _appointment;
        public AppointmentForm Appointment
        {
            get => _appointment;
            set { _appointment = value; OnPropertyChanged(nameof(Appointment)); }
        }

        // =====================================================================
        // LOADING & ERROR STATE
        // =====================================================================

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(nameof(SuccessMessage)); }
        }

        // =====================================================================
        // PATIENT SEARCH
        // =====================================================================

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set { _isSearching = value; OnPropertyChanged(nameof(IsSearching)); }
        }

        private CancellationTokenSource _searchCts;

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                
                // Debounce search
                _searchCts?.Cancel();
                _searchCts = new CancellationTokenSource();
                _ = DebouncedSearchPatientsAsync(_searchCts.Token);
            }
        }

        private bool _isPatientSelected;
        public bool IsPatientSelected
        {
            get => _isPatientSelected;
            set { _isPatientSelected = value; OnPropertyChanged(nameof(IsPatientSelected)); }
        }

        /// <summary>Search results shown in dropdown below search box</summary>
        public ObservableCollection<Patients> SearchResults { get; set; }

        private bool _showSearchResults;
        public bool ShowSearchResults
        {
            get => _showSearchResults;
            set { _showSearchResults = value; OnPropertyChanged(nameof(ShowSearchResults)); }
        }

        // Selected patient for the form (null = creating new)
        private Patients _selectedPatient;
        public Patients SelectedPatient
        {
            get => _selectedPatient;
            set { _selectedPatient = value; OnPropertyChanged(nameof(SelectedPatient)); }
        }

        // =====================================================================
        // ALL PATIENTS POPUP
        // =====================================================================

        private bool _isPatientPopupOpen;
        public bool IsPatientPopupOpen
        {
            get => _isPatientPopupOpen;
            set { _isPatientPopupOpen = value; OnPropertyChanged(nameof(IsPatientPopupOpen)); }
        }

        private bool _isLoadingPatients;
        public bool IsLoadingPatients
        {
            get => _isLoadingPatients;
            set { _isLoadingPatients = value; OnPropertyChanged(nameof(IsLoadingPatients)); }
        }

        public ObservableCollection<Patients> AllPatients { get; set; }

        // =====================================================================
        // NEW PATIENT FIELDS (shown when IsPatientSelected = false)
        // =====================================================================

        private string _newPatientName;
        public string NewPatientName
        {
            get => _newPatientName;
            set { _newPatientName = value; OnPropertyChanged(nameof(NewPatientName)); }
        }

        private string _newPatientPhone;
        public string NewPatientPhone
        {
            get => _newPatientPhone;
            set { _newPatientPhone = value; OnPropertyChanged(nameof(NewPatientPhone)); }
        }

        private string _newPatientGender = "Nam";
        public string NewPatientGender
        {
            get => _newPatientGender;
            set { _newPatientGender = value; OnPropertyChanged(nameof(NewPatientGender)); }
        }

        private DateTime? _newPatientDob;
        public DateTime? NewPatientDob
        {
            get => _newPatientDob;
            set { _newPatientDob = value; OnPropertyChanged(nameof(NewPatientDob)); }
        }

        private string _newPatientAddress;
        public string NewPatientAddress
        {
            get => _newPatientAddress;
            set { _newPatientAddress = value; OnPropertyChanged(nameof(NewPatientAddress)); }
        }

        private string _newPatientIdCard;
        public string NewPatientIdCard
        {
            get => _newPatientIdCard;
            set { _newPatientIdCard = value; OnPropertyChanged(nameof(NewPatientIdCard)); }
        }

        public ObservableCollection<string> GenderOptions { get; set; }

        // =====================================================================
        // DROPDOWNS (loaded from DB)
        // =====================================================================

        public ObservableCollection<string> AvailableDepartments { get; set; }
        public ObservableCollection<string> AvailableDoctors { get; set; }

        /// <summary>Full doctor objects for looking up ID by name</summary>
        private ObservableCollection<Doctors> _doctorList;

        // =====================================================================
        // APPOINTMENT TYPE (radio-style selection)
        // =====================================================================

        public bool IsTypeGeneral
        {
            get => Appointment?.AppointmentType == "Khám tổng quát";
            set { if (value) { Appointment.AppointmentType = "Khám tổng quát"; NotifyTypeChanged(); } }
        }
        public bool IsTypeFollowUp
        {
            get => Appointment?.AppointmentType == "Tái khám";
            set { if (value) { Appointment.AppointmentType = "Tái khám"; NotifyTypeChanged(); } }
        }
        public bool IsTypeEmergency
        {
            get => Appointment?.AppointmentType == "Cấp cứu";
            set { if (value) { Appointment.AppointmentType = "Cấp cứu"; NotifyTypeChanged(); } }
        }
        private void NotifyTypeChanged()
        {
            OnPropertyChanged(nameof(IsTypeGeneral));
            OnPropertyChanged(nameof(IsTypeFollowUp));
            OnPropertyChanged(nameof(IsTypeEmergency));
        }

        // =====================================================================
        // CALENDAR
        // =====================================================================

        private DateTime _displayMonth;
        public DateTime DisplayMonth
        {
            get => _displayMonth;
            set { _displayMonth = value; OnPropertyChanged(nameof(DisplayMonth)); OnPropertyChanged(nameof(MonthYearText)); BuildCalendarDays(); }
        }

        public string MonthYearText => $"Tháng {DisplayMonth.Month}, {DisplayMonth.Year}";

        public ObservableCollection<CalendarDay> CalendarDays { get; set; }

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                Appointment.SelectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                OnPropertyChanged(nameof(TodayText));
                OnPropertyChanged(nameof(SummaryTime));
                BuildCalendarDays();
                _ = RefreshBusySlots();
            }
        }

        public string TodayText
        {
            get
            {
                string dayOfWeek = SelectedDate.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Thứ 2",
                    DayOfWeek.Tuesday => "Thứ 3",
                    DayOfWeek.Wednesday => "Thứ 4",
                    DayOfWeek.Thursday => "Thứ 5",
                    DayOfWeek.Friday => "Thứ 6",
                    DayOfWeek.Saturday => "Thứ 7",
                    DayOfWeek.Sunday => "Chủ nhật",
                    _ => ""
                };
                return $"Hôm nay: {dayOfWeek}, {SelectedDate:dd/MM/yyyy}";
            }
        }

        // =====================================================================
        // TIME SLOTS
        // =====================================================================

        public ObservableCollection<TimeSlotItem> TimeSlots { get; set; }

        private string _selectedTimeSlot;
        public string SelectedTimeSlot
        {
            get => _selectedTimeSlot;
            set
            {
                _selectedTimeSlot = value;
                Appointment.SelectedTimeSlot = value;
                OnPropertyChanged(nameof(SelectedTimeSlot));
                OnPropertyChanged(nameof(SummaryTime));
                UpdateTimeSlotSelection();
            }
        }

        // =====================================================================
        // SUMMARY
        // =====================================================================

        public string SummaryTime => SelectedTimeSlot != null
            ? $"{SelectedTimeSlot.Split('-')[0].Trim()}, {SelectedDate:dd/MM/yyyy}"
            : "";

        // =====================================================================
        // COMMANDS
        // =====================================================================

        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ChangePatientCommand { get; }
        public ICommand SelectTimeSlotCommand { get; }
        public ICommand SelectDateCommand { get; }
        public ICommand PrevMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand SetAppointmentTypeCommand { get; }
        public ICommand SelectPatientCommand { get; }
        public ICommand OpenRegisterPatientCommand { get; }
        public ICommand OpenPatientPopupCommand { get; }
        public ICommand ClosePatientPopupCommand { get; }

        // =====================================================================
        // CONSTRUCTOR
        // =====================================================================

        public CreateAppointmentViewModel()
        {
            // Initialize repositories and services
            _registrationService = new AppointmentRegistrationService();
            _patientRepo = new PatientRepository();
            _appointmentRepo = new AppointmentRepository();
            _doctorRepo = new DoctorPanelRepository();

            // Initialize collections
            CalendarDays = new ObservableCollection<CalendarDay>();
            TimeSlots = new ObservableCollection<TimeSlotItem>();
            SearchResults = new ObservableCollection<Patients>();
            AllPatients = new ObservableCollection<Patients>();
            AvailableDepartments = new ObservableCollection<string>();
            AvailableDoctors = new ObservableCollection<string>();
            GenderOptions = new ObservableCollection<string> { "Nam", "Nữ", "Khác" };

            // Initialize form with defaults
            Appointment = new AppointmentForm
            {
                AppointmentType = "Khám tổng quát",
                SelectedDate = DateTime.Today,
                Location = "Tầng 2, Phòng 204"
            };
            Appointment.DepartmentChanged = OnDepartmentChanged;

            IsPatientSelected = false;
            _selectedDate = DateTime.Today;
            _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // Commands
            SubmitCommand = new RelayCommand(async _ =>
            {
                await ExecuteSubmit(_);
            });
            CancelCommand = new RelayCommand(ExecuteCancel);
            ChangePatientCommand = new RelayCommand(ExecuteChangePatient);
            SelectTimeSlotCommand = new RelayCommand(ExecuteSelectTimeSlot);
            SelectDateCommand = new RelayCommand(ExecuteSelectDate);
            PrevMonthCommand = new RelayCommand(_ => DisplayMonth = DisplayMonth.AddMonths(-1));
            NextMonthCommand = new RelayCommand(_ => DisplayMonth = DisplayMonth.AddMonths(1));
            SetAppointmentTypeCommand = new RelayCommand(ExecuteSetType);
            SelectPatientCommand = new RelayCommand(ExecuteSelectPatient);
            OpenRegisterPatientCommand = new RelayCommand(ExecuteOpenRegisterPatient);
            OpenPatientPopupCommand = new RelayCommand(ExecuteOpenPatientPopup);
            ClosePatientPopupCommand = new RelayCommand(_ => IsPatientPopupOpen = false);

            LoadTimeSlots();
            BuildCalendarDays();
        }

        // =====================================================================
        // INITIALIZATION (called from Window code-behind after construction)
        // =====================================================================

        /// <summary>
        /// Loads initial data from the database (doctors, departments).
        /// Called after the Window is created and DataContext is set.
        /// </summary>
        public async void InitializeAsync()
        {
            try
            {
                await LoadDoctorsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
            }
        }

        // =====================================================================
        // DATA LOADING FROM DB
        // =====================================================================

        private async Task LoadDoctorsAsync()
        {
            await Task.Run(() =>
            {
                var doctors = _doctorRepo.GetAllDoctors();
                _doctorList = doctors;

                var departments = doctors
                    .Where(d => !string.IsNullOrWhiteSpace(d.Specialization))
                    .Select(d => d.Specialization)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableDepartments.Clear();

                    foreach (var dept in departments)
                        AvailableDepartments.Add(dept);

                    AvailableDoctors.Clear();
                });
            });
        }

        private async Task DebouncedSearchPatientsAsync(CancellationToken token)
        {
            try
            {
                // Wait 300ms to debounce
                await Task.Delay(300, token);
            }
            catch (TaskCanceledException)
            {
                // Search cancelled by new keystroke
                return;
            }

            if (token.IsCancellationRequested) return;

            await SearchPatientsAsync();
        }

        private async Task SearchPatientsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchResults.Clear();
                    ShowSearchResults = false;
                    IsSearching = false;
                });
                return;
            }

            IsSearching = true;

            try
            {
                var results = await _patientRepo.SearchAsync(SearchText);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchResults.Clear();
                    foreach (var p in results)
                        SearchResults.Add(p);
                    ShowSearchResults = true; // Show dropdown even if empty (to show "Add New")
                });
            }
            catch
            {
                Application.Current.Dispatcher.Invoke(() => ShowSearchResults = false);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => IsSearching = false);
            }
        }

        private async Task ExecuteSubmit(object p)
        {
            ErrorMessage = "";
            SuccessMessage = "";

            // ===== VALIDATION =====
            string validationError = ValidateForm();
            if (!string.IsNullOrEmpty(validationError))
            {
                ErrorMessage = validationError;
                return;
            }

            IsLoading = true;

            try
            {
                string appointmentPatientId = null;

                // ===== BUILD PATIENT =====
                Patients patient;

                if (IsPatientSelected && SelectedPatient != null)
                {
                    // Existing patient
                    patient = SelectedPatient;
                    appointmentPatientId = patient.Id;
                }
                else
                {
                    // Create new patient
                    patient = new Patients
                    {
                        Name = NewPatientName?.Trim(),
                        Phone = NewPatientPhone?.Trim(),
                        Gender = NewPatientGender,
                        Dob = NewPatientDob ?? DateTime.Today,
                        Address = NewPatientAddress?.Trim(),
                        IdCard = NewPatientIdCard?.Trim()
                    };

                    // Generate patient ID
                    patient.Id = await _patientRepo.GenerateNextIdAsync();

                    appointmentPatientId =
                    patient.Id;

                }

                // ===== GET DOCTOR =====
                string doctorId = GetSelectedDoctorId();

                if (string.IsNullOrWhiteSpace(doctorId))
                {
                    ErrorMessage = "Vui lòng chọn bác sĩ phụ trách.";
                    IsLoading = false;
                    return;
                }

                // ===== BUILD APPOINTMENT =====
                var appointment = new Appointment
                {
                    Id = await _appointmentRepo.GenerateNextIdAsync(),

                    PatientId = appointmentPatientId,
                    DoctorId = doctorId,

                    AppointmentDate = SelectedDate,
                    AppointmentTime = SelectedTimeSlot,

                    Status = "Đang chờ",

                    Reason = Appointment.AppointmentType +
                             (string.IsNullOrWhiteSpace(Appointment.Note)
                                ? ""
                                : " - " + Appointment.Note),

                    CreatedBy = UserSession.CurrentUser?.ReceptionistId
                };
                var result =
                    await _registrationService
                    .RegisterAppointmentAsync(
                    patient,
                    appointment);

                if (!result.IsSuccess)
                {
                    ErrorMessage =
                    result.Message;

                    MessageBox.Show(
                        result.Message,
                        "Không thể tạo lịch hẹn",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                // ===== SAVE APPOINTMENT =====


                // ===== AUTO EXPORT PDF =====

                var appointmentDisplay = new AppointmentDisplay
                {
                    Id = appointment.Id,

                    PatientName = patient.Name,

                    PhoneNumber = patient.Phone,

                    DoctorName = Appointment.Doctor,

                    Department = Appointment.Department,

                    AppointmentDate = appointment.AppointmentDate,

                    AppointmentTime = appointment.AppointmentTime,

                    Status = appointment.Status,

                    Reason = Appointment.Note
                };

                var pdfService = new AppointmentPdfService();

                pdfService.Export(appointmentDisplay);
                System.Diagnostics.Debug.WriteLine(
                    $"Saved appointment: {appointment.Id}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.DataContext is ReceptionistDashboardViewModel dashboardVm)
                        {
                            dashboardVm.ShowSuccessToast(
                                "Tạo lịch hẹn",
                                "Lịch hẹn đã được tạo thành công"
                            );

                            dashboardVm.AddNotification(
                                "Lịch hẹn mới",
                                $"{patient.Name} - {SelectedTimeSlot}",
                                "success"
                            );

                            break;
                        }
                    }
                });

                CloseAction?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "DEBUG ERROR",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
        private async Task
RefreshBusySlots()
        {
            string doctorId =
            GetSelectedDoctorId();

            if (
            string.IsNullOrWhiteSpace(
            doctorId))
                return;

            var busy =
            await _appointmentRepo
            .GetDoctorBusySlotsAsync(
                doctorId,
                SelectedDate);

            foreach (
            var slot
            in TimeSlots)
            {
                string start =
                slot.Text
                .Split('-')[0]
                .Trim();

                slot.IsAvailable =
                !busy.Contains(start);

                if (
                !slot.IsAvailable
                &&
                slot.IsSelected)
                {
                    slot.IsSelected =
                    false;

                    SelectedTimeSlot =
                    null;
                }
            }
        }

        private bool CanExecuteSubmit(object p)
        {
            return !IsLoading;
        }

        private void ExecuteCancel(object p)
        {
            CloseAction?.Invoke(false);
        }

        private void ExecuteChangePatient(object p)
        {
            IsPatientSelected = false;
            SelectedPatient = null;
            SearchText = "";
            Appointment.PatientName = "";
            Appointment.PatientId = "";
            Appointment.PatientInitials = "";
            Appointment.PatientGender = "";
            Appointment.PatientAge = 0;
        }

        private void ExecuteSelectPatient(object p)
        {
            if (p is Patients patient)
            {
                SelectedPatient = patient;
                SearchText = patient.Name;
                IsPatientSelected = true;
                ShowSearchResults = false;
                IsPatientPopupOpen = false;

                // Update form display
                Appointment.PatientName = patient.Name;
                Appointment.PatientId = patient.Id;
                Appointment.PatientGender = patient.Gender;

                int age = DateTime.Today.Year - patient.Dob.Year;

if (patient.Dob.Date > DateTime.Today.AddYears(-age))
    age--;

Appointment.PatientAge = age;

Appointment.PatientInitials = GenerateInitials(patient.Name);

                OnPropertyChanged(nameof(Appointment));
            }
            ShowSearchResults = false;
            IsPatientPopupOpen = false;

            OnPropertyChanged(nameof(Appointment));
        }

        private void ExecuteSelectTimeSlot(object p)
        {
            if (p is string slot) SelectedTimeSlot = slot;
        }

        private void ExecuteSelectDate(object p)
        {
            if (p is DateTime date)
            {
                if (date.Date < DateTime.Today)
                {
                    ErrorMessage = "Không thể chọn ngày trong quá khứ.";
                    return;
                }
                ErrorMessage = "";
                SelectedDate = date;
            }
        }

        private void ExecuteSetType(object p)
        {
            if (p is string type) Appointment.AppointmentType = type;
            NotifyTypeChanged();
        }

        private async void ExecuteOpenPatientPopup(object p)
        {
            IsPatientPopupOpen = true;
            
            // Only load once
            if (AllPatients.Count > 0) return;

            IsLoadingPatients = true;
            try
            {
                var patients = await _patientRepo.GetAllAsync();
                
                // Thêm debug message như yêu cầu
                System.Windows.MessageBox.Show($"Loaded {patients.Count} patients");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AllPatients.Clear();
                    foreach (var patient in patients)
                        AllPatients.Add(patient);
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi tải danh sách bệnh nhân: {ex.Message}";
            }
            finally
            {
                IsLoadingPatients = false;
            }
        }

        private void ExecuteOpenRegisterPatient(object p)
        {
            // Close search dropdown
            ShowSearchResults = false;
            SearchText = "";

            // Open Modal Popup
            var window = new RegisterPatientWindow();
            window.Owner = Application.Current.MainWindow;

            // Retrieve DataContext to hook up callback
            if (window.DataContext is TamAnh_EMR_System.ViewModel.RegisterPatientViewModel vm)
            {
                vm.OnPatientSaved = (newPatient) =>
                {
                    // Callback when successfully saved
                    ExecuteSelectPatient(newPatient);
                    window.Close();
                };
            }

            window.ShowDialog();
        }

        // =====================================================================
        // VALIDATION
        // =====================================================================

        private string ValidateForm()
        {
            // Validate patient info
            if (!IsPatientSelected)
            {
                if (string.IsNullOrWhiteSpace(NewPatientName))
                    return "Vui lòng nhập họ tên bệnh nhân.";

                if (string.IsNullOrWhiteSpace(NewPatientPhone))
                    return "Vui lòng nhập số điện thoại bệnh nhân.";

                if (!IsValidPhone(NewPatientPhone))
                    return "Số điện thoại không hợp lệ. Vui lòng nhập 10-11 số bắt đầu bằng 0.";

                if (!NewPatientDob.HasValue)
                    return "Vui lòng chọn ngày sinh bệnh nhân.";

                if (NewPatientDob.Value.Date > DateTime.Today)
                    return "Ngày sinh không được lớn hơn ngày hiện tại.";
            }

            // Validate appointment info
            if (string.IsNullOrWhiteSpace(Appointment.Doctor))
                return "Vui lòng chọn bác sĩ phụ trách.";

            if (SelectedDate.Date < DateTime.Today)
                return "Ngày khám không thể ở trong quá khứ.";

            if (string.IsNullOrWhiteSpace(SelectedTimeSlot))
                return "Vui lòng chọn khung giờ khám.";

            return null; // No validation errors
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            // Vietnamese phone: starts with 0, 10-11 digits
            return Regex.IsMatch(phone.Trim(), @"^0\d{9,10}$");
        }

        // =====================================================================
        // HELPERS
        // =====================================================================

        private string GetSelectedDoctorId()
        {
            if (_doctorList == null || string.IsNullOrWhiteSpace(Appointment.Doctor))
                return null;

            var doctor = _doctorList.FirstOrDefault(d =>
                d.FullName == Appointment.Doctor);

            return doctor?.Id;
        }

        private string GenerateInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "??";
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        // =====================================================================
        // TIME SLOTS
        // =====================================================================

        private void LoadTimeSlots()
        {
            TimeSlots.Clear();
            string[] slots = {
                "08:00 - 09:00", "09:00 - 10:00",
                "10:00 - 11:00", "11:00 - 12:00",
                "14:00 - 15:00", "15:00 - 16:00",
                "16:00 - 17:00", "17:00 - 18:00"
            };

            foreach (var s in slots)
            {
                TimeSlots.Add(new TimeSlotItem
                {
                    Text = s,
                    IsAvailable = true,
                    IsSelected = false
                });
            }
        }

        private void UpdateTimeSlotSelection()
        {
            foreach (var ts in TimeSlots)
                ts.IsSelected = ts.Text == _selectedTimeSlot;
        }

        // =====================================================================
        // CALENDAR BUILDER
        // =====================================================================

        private void BuildCalendarDays()
        {
            CalendarDays.Clear();
            var first = new DateTime(DisplayMonth.Year, DisplayMonth.Month, 1);
            int startDow = ((int)first.DayOfWeek + 6) % 7; // Monday=0
            int daysInMonth = DateTime.DaysInMonth(DisplayMonth.Year, DisplayMonth.Month);

            // Previous month fill
            var prevMonth = first.AddDays(-1);
            int prevDays = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            for (int i = startDow - 1; i >= 0; i--)
                CalendarDays.Add(new CalendarDay { Day = prevDays - i, IsCurrentMonth = false });

            // Current month
            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateTime(DisplayMonth.Year, DisplayMonth.Month, d);
                CalendarDays.Add(new CalendarDay
                {
                    Day = d,
                    IsCurrentMonth = true,
                    IsSelected = date.Date == SelectedDate.Date,
                    FullDate = date
                });
            }

            // Next month fill (up to 42 cells for 6-row grid)
            int remaining = 42 - CalendarDays.Count;
            for (int i = 1; i <= remaining; i++)
                CalendarDays.Add(new CalendarDay { Day = i, IsCurrentMonth = false });
        }
        private void OnDepartmentChanged(string department)
        {
            if (_doctorList == null)
                return;

            AvailableDoctors.Clear();

            var doctors = _doctorList
                .Where(d => d.Specialization == department)
                .OrderBy(d => d.FullName)
                .ToList();

            foreach (var doctor in doctors)
            {
                AvailableDoctors.Add(doctor.FullName);
            }

            // reset selected doctor
            Appointment.Doctor = null;
            _ = RefreshBusySlots();
        }
    }

    /// <summary>Represents a single day cell in the calendar grid</summary>
    public class CalendarDay : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Day { get; set; }
        public bool IsCurrentMonth { get; set; }
        public DateTime FullDate { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); }
        }
    }

    /// <summary>Represents a selectable time slot</summary>
    public class TimeSlotItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Text { get; set; }
        public bool IsAvailable { get; set; } = true;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); }
        }
    }

}
