using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model.Receptionist;

namespace TamAnh_EMR_System.ViewModel.Receptionist
{
    /// <summary>
    /// ViewModel for "Tìm kiếm & Tiếp nhận bệnh nhân" screen.
    /// Manages search input, quick action cards, and recent patients table.
    /// FUTURE: Replace sample data with API service calls.
    /// </summary>
    public class SearchPatientViewModel : ViewModelBase
    {
        // =====================================================================
        // SEARCH
        // =====================================================================
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        // =====================================================================
        // STATS (blue card)
        // =====================================================================
        private int _checkedInToday;
        public int CheckedInToday
        {
            get => _checkedInToday;
            set { _checkedInToday = value; OnPropertyChanged(nameof(CheckedInToday)); }
        }

        private int _waitingCount;
        public int WaitingCount
        {
            get => _waitingCount;
            set { _waitingCount = value; OnPropertyChanged(nameof(WaitingCount)); }
        }

        // =====================================================================
        // RECENT PATIENTS TABLE
        // =====================================================================
        public ObservableCollection<PatientSearchResult> Patients { get; set; }

        // =====================================================================
        // COMMANDS
        // =====================================================================
        public ICommand SearchCommand { get; }
        public ICommand CheckInCommand { get; }
        public ICommand StartRegistrationCommand { get; }
        public ICommand ConfirmAppointmentCommand { get; }
        public ICommand ViewSearchHistoryCommand { get; }

        // =====================================================================
        // CONSTRUCTOR
        // =====================================================================
        public SearchPatientViewModel()
        {
            Patients = new ObservableCollection<PatientSearchResult>();
            SearchCommand = new RelayCommand(ExecuteSearch);
            CheckInCommand = new RelayCommand(ExecuteCheckIn);
            StartRegistrationCommand = new RelayCommand(_ =>
                MessageBox.Show("Mở form đăng ký bệnh nhân mới", "Đăng ký"));
            ConfirmAppointmentCommand = new RelayCommand(_ =>
                MessageBox.Show("Xử lý xác nhận lịch hẹn", "Xác nhận"));
            ViewSearchHistoryCommand = new RelayCommand(_ =>
                MessageBox.Show("Xem lịch sử tìm kiếm", "Lịch sử"));

            LoadSampleData();
        }

        private void ExecuteSearch(object p)
        {
            // TODO: Call API to search patients by SearchText
            MessageBox.Show($"Tìm kiếm: {SearchText}", "Tìm kiếm");
        }

        private void ExecuteCheckIn(object p)
        {
            if (p is PatientSearchResult patient && !patient.IsCheckedIn)
            {
                patient.IsCheckedIn = true;
            }
        }

        private void LoadSampleData()
        {
            CheckedInToday = 42;
            WaitingCount = 12;

            Patients.Add(new PatientSearchResult
            {
                Name = "Nguyen Vu", Initials = "ER", AvatarColor = "#3B82F6",
                PatientCode = "MRN-99812", DateOfBirthText = "12/05/1985",
                LastVisitText = "2 NGÀY TRƯỚC", LastVisitColor = "#6B7280", LastVisitBg = "#F3F4F6",
                IsCheckedIn = false
            });
            Patients.Add(new PatientSearchResult
            {
                Name = "Truong Hien", Initials = "SM", AvatarColor = "#10B981",
                PatientCode = "MRN-77231", DateOfBirthText = "02/10/1962",
                LastVisitText = "1 THÁNG TRƯỚC", LastVisitColor = "#6B7280", LastVisitBg = "#F3F4F6",
                IsCheckedIn = false
            });
            Patients.Add(new PatientSearchResult
            {
                Name = "Khai Bui", Initials = "JJ", AvatarColor = "#F59E0B",
                PatientCode = "MRN-10293", DateOfBirthText = "22/01/1998",
                LastVisitText = "HÔM NAY", LastVisitColor = "#10B981", LastVisitBg = "#ECFDF5",
                IsCheckedIn = true
            });
        }
    }
}
