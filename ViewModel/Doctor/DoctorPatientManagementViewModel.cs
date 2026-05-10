using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel.Doctor
{
    // Lớp Model dùng riêng để Binding lên UI của ListBox bên trái
    public class PatientQueueItem : ViewModelBase
    {
        public string PatientId { get; set; }
        public string AppointmentId { get; set; }
        public string Name { get; set; }
        public string Initials { get; set; }
        public string InfoString { get; set; } // VD: "Nam • 45 tuổi"
        public string Reason { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public string BloodType { get; set; }
        public string DOBString { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(nameof(IsActive)); }
        }
    }

    public class DoctorPatientManagementViewModel : ViewModelBase
    {
        private readonly DoctorPatientManagementRepository _repository;

        // Danh sách bệnh nhân (bên trái)
        public ObservableCollection<PatientQueueItem> PatientQueue { get; set; }

        // Bệnh nhân đang được chọn để khám (bên phải)
        private PatientQueueItem _selectedPatient;
        public PatientQueueItem SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                // Hủy active người cũ
                if (_selectedPatient != null) _selectedPatient.IsActive = false;

                _selectedPatient = value;

                // Active người mới
                if (_selectedPatient != null) _selectedPatient.IsActive = true;

                OnPropertyChanged(nameof(SelectedPatient));

                // Reset form khi đổi bệnh nhân
                DiagnosisText = "";
                NotesText = "";
            }
        }

        // --- Các trường dữ liệu trong Form Bệnh Án ---
        private string _diagnosisText;
        public string DiagnosisText
        {
            get => _diagnosisText;
            set { _diagnosisText = value; OnPropertyChanged(nameof(DiagnosisText)); }
        }

        private string _notesText;
        public string NotesText
        {
            get => _notesText;
            set { _notesText = value; OnPropertyChanged(nameof(NotesText)); }
        }

        // --- Commands ---
        public ICommand ScanQrCommand { get; }
        public ICommand SaveRecordCommand { get; }

        public DoctorPatientManagementViewModel()
        {
            _repository = new DoctorPatientManagementRepository();
            PatientQueue = new ObservableCollection<PatientQueueItem>();

            ScanQrCommand = new RelayCommand(_ => MessageBox.Show("Đang kết nối máy quét mã vạch...", "Quét mã"));
            SaveRecordCommand = new RelayCommand(ExecuteSaveRecord);

            _ = LoadQueueAsync();
        }

        private async Task LoadQueueAsync()
        {
            var data = await _repository.GetPatientsQueueAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                PatientQueue.Clear();
                foreach (var item in data)
                {
                    int age = DateTime.Now.Year - item.DOB.Year;
                    string initials = item.PatientName.Split(' ').LastOrDefault()?.Substring(0, 1).ToUpper() ?? "U";

                    PatientQueue.Add(new PatientQueueItem
                    {
                        PatientId = item.PatientId,
                        AppointmentId = item.AppointmentId,
                        Name = item.PatientName,
                        Initials = initials,
                        InfoString = $"{item.Gender} • {age} tuổi",
                        DOBString = $"{item.DOB:dd/MM/yyyy} ({age}t)",
                        Reason = item.Reason,
                        BloodType = item.BloodType,
                        Time = item.AppointmentTime.ToString(@"hh\:mm"),
                        Status = item.Status,
                        IsActive = false
                    });
                }

                // Mặc định chọn người đầu tiên
                if (PatientQueue.Count > 0)
                {
                    SelectedPatient = PatientQueue[0];
                }
            });
        }

        private async void ExecuteSaveRecord(object obj)
        {
            if (SelectedPatient == null)
            {
                MessageBox.Show("Vui lòng chọn một bệnh nhân để khám!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(DiagnosisText))
            {
                MessageBox.Show("Vui lòng nhập chuẩn đoán trước khi lưu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tạm thời hardcode DoctorID. Sau này bạn thay bằng ID của bác sĩ đang đăng nhập
            string currentDoctorId = "C1D2E3F4...";

            bool isSuccess = await _repository.SaveMedicalRecordAsync(SelectedPatient.PatientId, currentDoctorId, DiagnosisText, NotesText);

            if (isSuccess)
            {
                MessageBox.Show($"Đã lưu bệnh án cho bệnh nhân {SelectedPatient.Name} thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DiagnosisText = "";
                NotesText = "";
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi lưu vào Database.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}