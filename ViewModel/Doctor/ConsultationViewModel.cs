using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model.Doctor;

namespace TamAnh_EMR_System.ViewModel.Doctor
{
    /// <summary>
    /// ViewModel for Doctor Consultation screen (Khám bệnh / Bệnh án điện tử).
    /// Manages scan input, patient queue, consultation form with vitals/symptoms/diagnosis.
    /// FUTURE: Replace sample data with API calls.
    /// </summary>
    public class ConsultationViewModel : ViewModelBase
    {
        // ===== SCAN =====
        private string _scanCode;
        public string ScanCode { get => _scanCode; set { _scanCode = value; OnPropertyChanged(nameof(ScanCode)); } }

        // ===== QUEUE =====
        public ObservableCollection<QueuePatientItem> QueuePatients { get; set; }

        private int _waitingCount;
        public int WaitingCount { get => _waitingCount; set { _waitingCount = value; OnPropertyChanged(nameof(WaitingCount)); } }

        private QueuePatientItem _selectedPatient;
        public QueuePatientItem SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (_selectedPatient != null) _selectedPatient.IsSelected = false;
                _selectedPatient = value;
                if (_selectedPatient != null) _selectedPatient.IsSelected = true;
                OnPropertyChanged(nameof(SelectedPatient));
            }
        }

        // ===== CONSULTATION =====
        private ConsultationModel _consultation;
        public ConsultationModel Consultation { get => _consultation; set { _consultation = value; OnPropertyChanged(nameof(Consultation)); } }

        // ===== COMMANDS =====
        public ICommand ScanCommand { get; }
        public ICommand SelectPatientCommand { get; }
        public ICommand SaveRecordCommand { get; }
        public ICommand CreatePrescriptionCommand { get; }
        public ICommand ViewHistoryCommand { get; }

        public ConsultationViewModel()
        {
            QueuePatients = new ObservableCollection<QueuePatientItem>();

            ScanCommand = new RelayCommand(_ => MessageBox.Show($"Quét mã: {ScanCode}", "Quét mã"));
            SelectPatientCommand = new RelayCommand(p => { if (p is QueuePatientItem q) SelectedPatient = q; });
            SaveRecordCommand = new RelayCommand(_ => MessageBox.Show("Đã lưu bệnh án!", "Lưu bệnh án", MessageBoxButton.OK, MessageBoxImage.Information));
            CreatePrescriptionCommand = new RelayCommand(_ => MessageBox.Show("Mở kê toa thuốc", "Kê toa"));
            ViewHistoryCommand = new RelayCommand(_ => MessageBox.Show("Xem lịch sử khám", "Lịch sử"));

            LoadSampleData();
        }

        private void LoadSampleData()
        {
            WaitingCount = 4;

            QueuePatients.Add(new QueuePatientItem { Name = "Nguyễn Văn A", Initials = "NV", AvatarColor = "#6366F1", GenderAge = "Nam • 45 tuổi", Time = "08:30", Status = "ĐANG KHÁM", Reason = "Sốt cao, ho khan", IsSelected = true });
            QueuePatients.Add(new QueuePatientItem { Name = "Trần Thị B", Initials = "TB", AvatarColor = "#3B82F6", GenderAge = "Nữ • 32 tuổi", Time = "09:00", Status = "Khám bệnh", Reason = "Đau dạ dày" });
            QueuePatients.Add(new QueuePatientItem { Name = "Lê Hoàng M", Initials = "LM", AvatarColor = "#3B82F6", GenderAge = "Nam • 12 tuổi", Time = "09:30", Status = "Khám bệnh", Reason = "Tái khám viêm họng" });

            _selectedPatient = QueuePatients[0];

            Consultation = new ConsultationModel
            {
                PatientName = "Nguyễn Văn A",
                PatientId = "BN-2023-0891",
                Gender = "Nam",
                Age = 45,
                BirthDate = "15/04/1978",
                BloodType = "O+",
                Insurance = "BHYT HVp IS",
                VitalSigns = new VitalSigns { Pulse = 85, BloodPressure = "120/80", Temperature = 38.5, SpO2 = 98 },
                Diagnosis = "Viêm họng cấp, theo dõi cúm A.",
                Symptoms = new ObservableCollection<SymptomItem>
                {
                    new SymptomItem { Name = "Sốt", IsSelected = true },
                    new SymptomItem { Name = "Ho khan", IsSelected = true },
                    new SymptomItem { Name = "Khó thở", IsSelected = false },
                    new SymptomItem { Name = "Đau đầu", IsSelected = false },
                    new SymptomItem { Name = "Buồn nôn", IsSelected = false }
                }
            };
        }
    }
}
