using System.Windows;
using System.Linq;
using System.Collections.Generic;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.View.Doctor
{
    public partial class MedicalRecordDetailWindow : Window
    {
        public MedicalRecords RecordData { get; private set; }
        public bool IsSaved { get; private set; } = false;

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
                Doctor = record.Doctor,
                // Copy danh sách xét nghiệm sang
                LabResults = record.LabResults,

                PrescriptionDetails = record.PrescriptionDetails
            };
            this.DataContext = RecordData;

            // Xử lý hiển thị dữ liệu lên 2 ô TextBox mới bằng LINQ
            if (RecordData.LabResults != null && RecordData.LabResults.Any())
            {
                var firstLab = RecordData.LabResults.FirstOrDefault();
                if (firstLab != null)
                {
                    TxtLabTestName.Text = firstLab.TestName;
                    TxtLabResult.Text = firstLab.Result;
                }
            }
            else
            {
                // Nếu chưa có xét nghiệm nào thì khởi tạo sẵn list trống để lúc Lưu không bị lỗi
                RecordData.LabResults = new List<LabResults> { new LabResults() };
            }

            // NẾU LÀ CHẾ ĐỘ XEM CHI TIẾT (isEditMode == false) -> KHÓA CÁC Ô NHẬP LIỆU
            if (isEditMode == false)
            {
                this.Title = "Chi Tiết Bệnh Án";
                BtnSave.Visibility = Visibility.Collapsed; // Ẩn nút lưu
                BtnExportPdf.Visibility = Visibility.Visible;
                TxtPulse.IsReadOnly = true;
                TxtBloodPressure.IsReadOnly = true;
                TxtTemperature.IsReadOnly = true;
                TxtSPO2.IsReadOnly = true;
                TxtIcdCode.IsReadOnly = true;
                TxtDiagnosis.IsReadOnly = true;
                TxtTreatment.IsReadOnly = true;
                TxtNotes.IsReadOnly = true;

                // Khóa luôn 2 ô xét nghiệm
                TxtLabTestName.IsReadOnly = true;
                TxtLabResult.IsReadOnly = true;

                var readOnlyBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3F4F6"));
                TxtPulse.Background = readOnlyBrush;
                TxtBloodPressure.Background = readOnlyBrush;
                TxtTemperature.Background = readOnlyBrush;
                TxtSPO2.Background = readOnlyBrush;
                TxtIcdCode.Background = readOnlyBrush;
                TxtDiagnosis.Background = readOnlyBrush;
                TxtTreatment.Background = readOnlyBrush;
                TxtNotes.Background = readOnlyBrush;

                // Tô nền xám cho 2 ô xét nghiệm
                TxtLabTestName.Background = readOnlyBrush;
                TxtLabResult.Background = readOnlyBrush;
            }
            else
            {
                this.Title = "Chỉnh Sửa Bệnh Án";
                BtnExportPdf.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Lấy ra phần tử đầu tiên để cập nhật dữ liệu từ TextBox trước khi đóng form
            var labToUpdate = RecordData.LabResults?.FirstOrDefault();
            if (labToUpdate != null)
            {
                labToUpdate.TestName = TxtLabTestName.Text;
                labToUpdate.Result = TxtLabResult.Text;
            }

            IsSaved = true;
            this.DialogResult = true;
            this.Close();
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Cập nhật dữ liệu đang gõ dở trên màn hình vào Object để xuất ra file
                RecordData.Pulse = TxtPulse.Text;
                RecordData.BloodPressure = TxtBloodPressure.Text;
                RecordData.Temperature = TxtTemperature.Text;
                RecordData.SPO2 = TxtSPO2.Text;

                RecordData.IcdCode = TxtIcdCode.Text;
                RecordData.Diagnosis = TxtDiagnosis.Text;
                RecordData.Treatment = TxtTreatment.Text;
                RecordData.Notes = TxtNotes.Text;

                var labToUpdate = RecordData.LabResults?.FirstOrDefault();
                if (labToUpdate != null)
                {
                    labToUpdate.TestName = TxtLabTestName.Text;
                    labToUpdate.Result = TxtLabResult.Text;
                }

                // Gọi class Service để tạo PDF
                var pdfService = new TamAnh_EMR_System.Services.Pdf.MedicalRecordPdfService();
                pdfService.Export(RecordData);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra khi xuất PDF:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}