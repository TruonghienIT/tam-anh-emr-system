using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model.Receptionist;

namespace TamAnh_EMR_System.ViewModel.Receptionist
{
    /// <summary>
    /// ViewModel for "Tạo lịch hẹn mới" screen.
    /// Manages appointment form data, calendar navigation, time slot selection,
    /// department/doctor dropdowns, and all button commands.
    /// FUTURE: Replace sample data with API service calls.
    /// </summary>
    public class CreateAppointmentViewModel : ViewModelBase
    {
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
        // SEARCH
        // =====================================================================

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        private bool _isPatientSelected;
        public bool IsPatientSelected
        {
            get => _isPatientSelected;
            set { _isPatientSelected = value; OnPropertyChanged(nameof(IsPatientSelected)); }
        }

        // =====================================================================
        // DROPDOWNS
        // =====================================================================

        public ObservableCollection<string> AvailableDepartments { get; set; }
        public ObservableCollection<string> AvailableDoctors { get; set; }

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

        /// <summary>Calendar day cells for the grid</summary>
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
            }
        }

        public string TodayText
        {
            get
            {
                var culture = new CultureInfo("vi-VN");
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

        // =====================================================================
        // CONSTRUCTOR
        // =====================================================================

        public CreateAppointmentViewModel()
        {
            CalendarDays = new ObservableCollection<CalendarDay>();
            TimeSlots = new ObservableCollection<TimeSlotItem>();

            // Initialize form with sample patient
            Appointment = new AppointmentForm
            {
                PatientName = "Nguyễn Văn An",
                PatientId = "BN-2023-0892",
                PatientInitials = "NV",
                PatientGender = "Nam",
                PatientAge = 28,
                Department = "Nội tổng quát",
                Doctor = "BS. Lê Hoàng Nam",
                AppointmentType = "Khám tổng quát",
                SelectedDate = new DateTime(2023, 11, 6),
                SelectedTimeSlot = "09:00 - 09:30",
                Location = "Tầng 2, Phòng 204"
            };

            IsPatientSelected = true;
            _selectedDate = new DateTime(2023, 11, 6);
            _displayMonth = new DateTime(2023, 11, 1);
            _selectedTimeSlot = "09:00 - 09:30";

            // Dropdowns
            AvailableDepartments = new ObservableCollection<string>
            {
                "Nội tổng quát", "Ngoại khoa", "Sản phụ khoa",
                "Nhi khoa", "Tai Mũi Họng", "Răng Hàm Mặt", "Tim mạch"
            };

            AvailableDoctors = new ObservableCollection<string>
            {
                "BS. Lê Hoàng Nam", "BS. Trần Đức Anh", "BS. Lê Thu Thủy",
                "BS. Nguyễn Văn A", "BS. Phạm Văn C"
            };

            // Commands
            SubmitCommand = new RelayCommand(ExecuteSubmit);
            CancelCommand = new RelayCommand(ExecuteCancel);
            ChangePatientCommand = new RelayCommand(ExecuteChangePatient);
            SelectTimeSlotCommand = new RelayCommand(ExecuteSelectTimeSlot);
            SelectDateCommand = new RelayCommand(ExecuteSelectDate);
            PrevMonthCommand = new RelayCommand(_ => DisplayMonth = DisplayMonth.AddMonths(-1));
            NextMonthCommand = new RelayCommand(_ => DisplayMonth = DisplayMonth.AddMonths(1));
            SetAppointmentTypeCommand = new RelayCommand(ExecuteSetType);

            LoadTimeSlots();
            BuildCalendarDays();
        }

        // =====================================================================
        // COMMAND IMPLEMENTATIONS
        // =====================================================================

        private void ExecuteSubmit(object p)
        {
            MessageBox.Show($"Đã gửi lịch hẹn cho {Appointment.PatientName}\n" +
                $"Thời gian: {SummaryTime}\nĐịa điểm: {Appointment.Location}",
                "Gửi lịch hẹn", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteCancel(object p)
        {
            MessageBox.Show("Hủy bỏ tạo lịch hẹn", "Hủy bỏ");
        }

        private void ExecuteChangePatient(object p)
        {
            IsPatientSelected = false;
            SearchText = "";
        }

        private void ExecuteSelectTimeSlot(object p)
        {
            if (p is string slot) SelectedTimeSlot = slot;
        }

        private void ExecuteSelectDate(object p)
        {
            if (p is DateTime date) SelectedDate = date;
        }

        private void ExecuteSetType(object p)
        {
            if (p is string type) Appointment.AppointmentType = type;
            NotifyTypeChanged();
        }

        // =====================================================================
        // DATA LOADERS
        // =====================================================================

        private void LoadTimeSlots()
        {
            TimeSlots.Clear();
            string[] slots = {
                "08:00 - 08:30", "08:30 - 09:00",
                "09:00 - 09:30", "09:30 - 10:00",
                "10:00 - 10:30", "10:30 - 11:00",
                "14:00 - 14:30", "14:30 - 15:00"
            };
            string[] disabled = { "10:00 - 10:30" };

            foreach (var s in slots)
            {
                TimeSlots.Add(new TimeSlotItem
                {
                    Text = s,
                    IsAvailable = !Array.Exists(disabled, d => d == s),
                    IsSelected = s == _selectedTimeSlot
                });
            }
        }

        private void UpdateTimeSlotSelection()
        {
            foreach (var ts in TimeSlots)
                ts.IsSelected = ts.Text == _selectedTimeSlot;
        }

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
