using System.Windows;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.View.Doctor
{
    public partial class MedicalRecordDetailWindow : Window
    {
        public MedicalRecords RecordData { get; private set; }
        public bool IsSaved { get; private set; } = false;

        // Thêm tham số bool isEditMode
        public MedicalRecordDetailWindow(MedicalRecords record, bool isEditMode)
        {
            InitializeComponent();

            RecordData = new MedicalRecords
            {
                Id = record.Id,
                PatientId = record.PatientId,
                DoctorId = record.DoctorId,
                IcdCode = record.IcdCode,
                Diagnosis = record.Diagnosis,
                Treatment = record.Treatment,
                Notes = record.Notes,
                Pulse = record.Pulse,
                BloodPressure = record.BloodPressure,
                Temperature = record.Temperature,
                SPO2 = record.SPO2,
                CreatedAt = record.CreatedAt,
                Patient = record.Patient,
                Doctor = record.Doctor
            };
            this.DataContext = RecordData;

            // NẾU LÀ CHẾ ĐỘ XEM CHI TIẾT (isEditMode == false) -> KHÓA CÁC Ô NHẬP LIỆU
            if (isEditMode == false)
            {
                this.Title = "Chi Tiết Bệnh Án";
                BtnSave.Visibility = Visibility.Collapsed; // Ẩn nút lưu

                TxtPulse.IsReadOnly = true;
                TxtBloodPressure.IsReadOnly = true;
                TxtTemperature.IsReadOnly = true;
                TxtSPO2.IsReadOnly = true;
                TxtIcdCode.IsReadOnly = true;
                TxtDiagnosis.IsReadOnly = true;
                TxtTreatment.IsReadOnly = true;
                TxtNotes.IsReadOnly = true;

                // Tô nền xám cho các ô để biểu thị trạng thái không được sửa
                var readOnlyBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                TxtPulse.Background = readOnlyBrush;
                TxtBloodPressure.Background = readOnlyBrush;
                TxtTemperature.Background = readOnlyBrush;
                TxtSPO2.Background = readOnlyBrush;
                TxtIcdCode.Background = readOnlyBrush;
                TxtDiagnosis.Background = readOnlyBrush;
                TxtTreatment.Background = readOnlyBrush;
                TxtNotes.Background = readOnlyBrush;
            }
            else
            {
                this.Title = "Chỉnh Sửa Bệnh Án";
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = true;
            this.DialogResult = true;
            this.Close();
        }
    }
}