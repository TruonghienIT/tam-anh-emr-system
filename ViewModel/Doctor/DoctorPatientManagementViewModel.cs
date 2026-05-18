using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;
using TamAnh_EMR_System.Services;
using PdfiumViewer;
using System.Drawing;
using ZXing;
using ZXing.Windows.Compatibility;

namespace TamAnh_EMR_System.ViewModel.Doctor
{
    public class PatientQueueItem : ViewModelBase
    {
        public string PatientId { get; set; }
        public string AppointmentId { get; set; }
        public string DoctorId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Initials { get; set; }
        public string InfoString { get; set; }
        public string Reason { get; set; }
        public string Time { get; set; }
        public string BloodType { get; set; }
        public string DOBString { get; set; }

        public ObservableCollection<string> StatusOptions { get; set; } =
            new ObservableCollection<string>
            {
                "Đang chờ",
                "Đang khám",
                "Hoàn thành",
                "Đã hủy"
            };

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    StatusChanged?.Invoke(this, value);
                }
            }
        }

        public Action<PatientQueueItem, string> StatusChanged { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
    }

    public class DoctorPatientManagementViewModel : ViewModelBase
    {
        private readonly DoctorPatientManagementRepository _repository;

        private readonly QrDecoderService _qrService;

        public ObservableCollection<PatientQueueItem> PatientQueue { get; set; }

        public int WaitingCount => PatientQueue.Count(p => p.Status == "Đang chờ");

        #region Selected Patient
        private PatientQueueItem _selectedPatient;
        public PatientQueueItem SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (_selectedPatient != null)
                    _selectedPatient.IsActive = false;

                _selectedPatient = value;

                if (_selectedPatient != null)
                    _selectedPatient.IsActive = true;

                OnPropertyChanged(nameof(SelectedPatient));

                ClearMedicalForm();
            }
        }
        #endregion

        #region Vital Signs
        private string _pulse;
        public string Pulse
        {
            get => _pulse;
            set
            {
                _pulse = value;
                OnPropertyChanged(nameof(Pulse));
            }
        }

        private string _bloodPressure;
        public string BloodPressure
        {
            get => _bloodPressure;
            set
            {
                _bloodPressure = value;
                OnPropertyChanged(nameof(BloodPressure));
            }
        }

        private string _temperature;
        public string Temperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged(nameof(Temperature));
            }
        }

        private string _spo2;
        public string SPO2
        {
            get => _spo2;
            set
            {
                _spo2 = value;
                OnPropertyChanged(nameof(SPO2));
            }
        }
        #endregion

        #region Lab
        private string _labTestName;
        public string LabTestName
        {
            get => _labTestName;
            set
            {
                _labTestName = value;
                OnPropertyChanged(nameof(LabTestName));
            }
        }

        private string _labResult;
        public string LabResult
        {
            get => _labResult;
            set
            {
                _labResult = value;
                OnPropertyChanged(nameof(LabResult));
            }
        }
        #endregion

        #region Diagnosis
        private string _diseaseCode;
        public string DiseaseCode
        {
            get => _diseaseCode;
            set
            {
                _diseaseCode = value;
                OnPropertyChanged(nameof(DiseaseCode));
            }
        }

        private string _diseaseName;
        public string DiseaseName
        {
            get => _diseaseName;
            set
            {
                _diseaseName = value;
                OnPropertyChanged(nameof(DiseaseName));
            }
        }

        private string _diagnosisDescription;
        public string DiagnosisDescription
        {
            get => _diagnosisDescription;
            set
            {
                _diagnosisDescription = value;
                OnPropertyChanged(nameof(DiagnosisDescription));
            }
        }
        #endregion

        #region Treatment
        private string _treatmentPlan;
        public string TreatmentPlan
        {
            get => _treatmentPlan;
            set
            {
                _treatmentPlan = value;
                OnPropertyChanged(nameof(TreatmentPlan));
            }
        }
        #endregion

        #region Notes
        private string _notesText;
        public string NotesText
        {
            get => _notesText;
            set
            {
                _notesText = value;
                OnPropertyChanged(nameof(NotesText));
            }
        }
        #endregion


        public ICommand ScanQrCommand { get; }
        public ICommand SaveRecordCommand { get; }
        public ICommand ImportQrImageCommand { get; }

        public DoctorPatientManagementViewModel()
        {
            _repository = new DoctorPatientManagementRepository();
            _qrService = new QrDecoderService();

            PatientQueue = new ObservableCollection<PatientQueueItem>();

            ScanQrCommand = new RelayCommand(_ =>
                MessageBox.Show(
                    "Đang kết nối máy quét mã QR...",
                    "Quét mã",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information));

            SaveRecordCommand = new RelayCommand(ExecuteSaveRecord);

            ImportQrImageCommand = new RelayCommand(ExecuteImportQrImage);

            _ = LoadQueueAsync();

            _ = LoadDiseasesAsync();
        }

        #region ICD SELECT
        public ObservableCollection<Diseases> DiseaseList { get; set; }
            = new ObservableCollection<Diseases>();

        private Diseases _selectedDisease;
        public Diseases SelectedDisease
        {
            get => _selectedDisease;
            set
            {
                _selectedDisease = value;
                OnPropertyChanged(nameof(SelectedDisease));

                if (_selectedDisease != null)
                {
                    DiseaseCode = _selectedDisease.IcdCode;
                    DiseaseName = _selectedDisease.DiseaseName;
                    DiagnosisDescription = _selectedDisease.Description;
                }
            }
        }
        #endregion

        #region Load Queue
        private async Task LoadQueueAsync()
        {
            var data = await _repository.GetPatientsQueueAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                PatientQueue.Clear();

                foreach (var item in data)
                {
                    int age = DateTime.Now.Year - item.DOB.Year;
                    if (DateTime.Now.DayOfYear < item.DOB.DayOfYear)
                        age--;

                    string initials = item.PatientName
                        .Split(' ')
                        .LastOrDefault()?
                        .Substring(0, 1)
                        .ToUpper() ?? "U";

                    var patient = new PatientQueueItem
                    {
                        PatientId = item.PatientId,
                        AppointmentId = item.AppointmentId,
                        DoctorId = item.DoctorId,

                        Name = item.PatientName,
                        Initials = initials,
                        InfoString = $"{item.Gender} • {age} tuổi",
                        DOBString = $"{item.DOB:dd/MM/yyyy} ({age}t)",
                        Reason = item.Reason,
                        BloodType = item.BloodType,
                        Time = item.AppointmentTime.ToString(@"hh\:mm"),
                        Status = string.IsNullOrEmpty(item.Status)
                            ? "Đang chờ"
                            : item.Status,
                        PhoneNumber = item.PhoneNumber,
                        IsActive = false
                    };

                    patient.StatusChanged = async (p, newStatus) =>
                    {
                        await UpdatePatientStatusAsync(p, newStatus);
                        OnPropertyChanged(nameof(WaitingCount));
                    };

                    PatientQueue.Add(patient);
                }

                if (PatientQueue.Count > 0)
                    SelectedPatient = PatientQueue[0];

                OnPropertyChanged(nameof(WaitingCount));
            });
        }
        #endregion

        #region LOAD DISEASES
        private async Task LoadDiseasesAsync()
        {
            var data = await _repository.GetDiseasesAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                DiseaseList.Clear();

                foreach (var d in data)
                {
                    DiseaseList.Add(d);
                }
            });
        }
        #endregion


        #region Update Status
        private async Task UpdatePatientStatusAsync(
            PatientQueueItem patient,
            string newStatus)
        {
            bool success = await _repository.UpdatePatientStatusAsync(
                patient.AppointmentId,
                newStatus
            );

            if (!success)
            {
                MessageBox.Show(
                    $"Không thể cập nhật trạng thái cho {patient.Name}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            OnPropertyChanged(nameof(WaitingCount));
        }
        #endregion

        #region Save Medical Record
        private async void ExecuteSaveRecord(object obj)
        {
            if (SelectedPatient == null)
            {
                MessageBox.Show(
                    "Vui lòng chọn bệnh nhân!",
                    "Cảnh báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (string.IsNullOrWhiteSpace(DiagnosisDescription))
            {
                MessageBox.Show(
                    "Vui lòng nhập chuẩn đoán!",
                    "Cảnh báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            string currentDoctorId = SelectedPatient.DoctorId;

            if (string.IsNullOrWhiteSpace(currentDoctorId))
            {
                MessageBox.Show(
                    "Không tìm thấy bác sĩ trong lịch hẹn!",
                    "Lỗi hệ thống",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            bool isSuccess = await _repository.SaveMedicalRecordAsync(
                SelectedPatient.PatientId,
                SelectedPatient.AppointmentId,
                currentDoctorId,
                DiseaseCode,
                DiagnosisDescription,
                TreatmentPlan,
                NotesText,
                Pulse,
                BloodPressure,
                Temperature,
                SPO2,
                LabTestName,
                LabResult
            );

            if (isSuccess)
            {
                SelectedPatient.Status = "Hoàn thành";

                MessageBox.Show(
                    $"Đã lưu bệnh án cho bệnh nhân {SelectedPatient.Name} thành công!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                ClearMedicalForm();

                OnPropertyChanged(nameof(WaitingCount));
            }
            else
            {
                MessageBox.Show(
                    "Có lỗi xảy ra khi lưu bệnh án!",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        #endregion

        #region Helpers
        private void ClearMedicalForm()
        {
            Pulse = "";
            BloodPressure = "";
            Temperature = "";
            SPO2 = "";

            LabTestName = "";
            LabResult = "";

            DiseaseCode = "";
            DiseaseName = "";
            DiagnosisDescription = "";

            TreatmentPlan = "";
            NotesText = "";
        }
        #endregion

        private void ExecuteImportQrImage(object obj)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf|Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                string qrText = null;

                if (dialog.FileName.EndsWith(".pdf"))
                {
                    qrText = DecodeQrFromPdf(dialog.FileName);
                }
                else
                {
                    qrText = _qrService.DecodeFromImage(dialog.FileName);
                }

                if (string.IsNullOrWhiteSpace(qrText))
                {
                    MessageBox.Show(
                        "Không tìm thấy mã QR!",
                        "QR Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                HandleQrResult(qrText);
            }
        }

        private void HandleQrResult(string qr)
        {
            try
            {
                var data = JsonSerializer.Deserialize<AppointmentQrDto>(qr);

                if (data == null)
                {
                    MessageBox.Show("QR không hợp lệ!");
                    return;
                }

                var patient = PatientQueue
                    .FirstOrDefault(x => x.AppointmentId == data.appointmentId);

                if (patient != null)
                {
                    SelectedPatient = patient;
                }
                else
                {
                    MessageBox.Show(
                        $"Không tìm thấy lịch hẹn: {data.appointmentId}",
                        "Không hợp lệ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch
            {
                MessageBox.Show(
                    "QR không đúng định dạng JSON!",
                    "QR Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private string DecodeQrFromPdf(string pdfPath)
        {
            using var pdf = PdfDocument.Load(pdfPath);

            using var image = pdf.Render(0, 300, 300, true);

            using var bitmap = new Bitmap(image);

            var barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                TryInverted = true
            };

            var result = barcodeReader.Decode(bitmap);

            return result?.Text;
        }
    }
}