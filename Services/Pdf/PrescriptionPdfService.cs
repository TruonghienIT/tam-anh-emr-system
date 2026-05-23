using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Infrastructure;
using TamAnh_EMR_System.Model.Doctor;

namespace TamAnh_EMR_System.Services.Pdf
{
    public class PrescriptionPdfService
    {
        public void Export(Prescription prescription, List<MedicineItem> medicines)
        {
            if (prescription == null)
                throw new ArgumentNullException(nameof(prescription));

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.PageColor("#F8FAFC");
                    page.DefaultTextStyle(x =>
                        x.FontSize(12)
                         .FontColor("#111827"));

                    // ================= HEADER =================
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item()
                                .Text("TÂM ANH HOSPITAL")
                                .Bold()
                                .FontSize(24)
                                .FontColor("#2563EB");

                            col.Item()
                                .PaddingTop(4)
                                .Text("ĐƠN KHAI THUỐC")
                                .SemiBold()
                                .FontSize(14)
                                .FontColor("#6B7280");
                        });

                        row.ConstantItem(120)
                            .AlignRight()
                            .Column(col =>
                            {
                                col.Item()
                                    .Text($"#{prescription.RecordId}")
                                    .Bold()
                                    .FontSize(18)
                                    .FontColor("#9CA3AF");

                                col.Item()
                                    .PaddingTop(4)
                                    .Text($"{DateTime.Now:dd/MM/yyyy}")
                                    .FontSize(11)
                                    .FontColor("#6B7280");
                            });
                    });

                    // ================= CONTENT =================
                    page.Content().PaddingVertical(24).Column(col =>
                    {
                        // PATIENT INFO CARD
                        col.Item()
                            .Background(Colors.White)
                            .Border(1)
                            .BorderColor("#E5E7EB")
                            .CornerRadius(8)
                            .Padding(20)
                            .Column(info =>
                            {
                                info.Item()
                                    .Text("THÔNG TIN BỆNH NHÂN")
                                    .SemiBold()
                                    .FontSize(13)
                                    .FontColor("#6B7280");

                                info.Item().PaddingTop(12);

                                info.Item()
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Tên bệnh nhân:").FontSize(11).FontColor("#6B7280");
                                                c.Item().PaddingTop(2).Text(prescription.PatientName)
                                                    .Bold().FontSize(13).FontColor("#111827");
                                            });

                                        row.RelativeItem()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Mã bệnh nhân:").FontSize(11).FontColor("#6B7280");
                                                c.Item().PaddingTop(2).Text(prescription.PatientId)
                                                    .Bold().FontSize(13).FontColor("#111827");
                                            });
                                    });

                                info.Item()
                                    .PaddingTop(12)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Ngày khám:").FontSize(11).FontColor("#6B7280");
                                                c.Item().PaddingTop(2).Text($"{prescription.Date:dd/MM/yyyy HH:mm}")
                                                    .Bold().FontSize(13).FontColor("#111827");
                                            });

                                        row.RelativeItem()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Bác sĩ điều trị:").FontSize(11).FontColor("#6B7280");
                                                c.Item().PaddingTop(2).Text(prescription.DoctorName)
                                                    .Bold().FontSize(13).FontColor("#111827");
                                            });
                                    });
                            });

                        // MEDICINE LIST
                        col.Item().PaddingTop(24)
                            .Text("CHI TIẾT THUỐC")
                            .SemiBold()
                            .FontSize(13)
                            .FontColor("#6B7280");

                        col.Item().PaddingTop(12)
                            .Background(Colors.White)
                            .Border(1)
                            .BorderColor("#E5E7EB")
                            .CornerRadius(8)
                            .Padding(20)
                            .Column(medicineCol =>
                            {
                                if (medicines == null || medicines.Count == 0)
                                {
                                    medicineCol.Item()
                                        .Text("Không có thuốc nào trong đơn này")
                                        .FontSize(12)
                                        .FontColor("#9CA3AF");
                                }
                                else
                                {
                                    // Header row
                                    medicineCol.Item()
                                        .BorderBottom(1)
                                        .BorderColor("#F3F4F6")
                                        .PaddingBottom(10)
                                        .Row(headerRow =>
                                        {
                                            headerRow.RelativeItem(2).Text("Tên thuốc")
                                                .SemiBold().FontSize(11).FontColor("#6B7280");
                                            headerRow.RelativeItem(1).Text("Liều dùng")
                                                .SemiBold().FontSize(11).FontColor("#6B7280");
                                            headerRow.RelativeItem(1).Text("Tần suất")
                                                .SemiBold().FontSize(11).FontColor("#6B7280");
                                            headerRow.RelativeItem(1).Text("Số lượng")
                                                .SemiBold().FontSize(11).FontColor("#6B7280");
                                            headerRow.RelativeItem(2).Text("Hướng dẫn")
                                                .SemiBold().FontSize(11).FontColor("#6B7280");
                                        });

                                    // Medicine items
                                    for (int i = 0; i < medicines.Count; i++)
                                    {
                                        var medicine = medicines[i];
                                        medicineCol.Item()
                                            .PaddingVertical(10)
                                            .BorderBottom(i < medicines.Count - 1 ? 1 : 0)
                                            .BorderColor("#F3F4F6")
                                            .Row(row =>
                                            {
                                                row.RelativeItem(2).Column(c =>
                                                {
                                                    c.Item().Text(medicine.Name)
                                                        .SemiBold().FontSize(11).FontColor("#111827");
                                                });

                                                row.RelativeItem(1).Column(c =>
                                                {
                                                    c.Item().Text(medicine.Dosage ?? "-")
                                                        .FontSize(11).FontColor("#374151");
                                                });

                                                row.RelativeItem(1).Column(c =>
                                                {
                                                    c.Item().Text(medicine.Frequency ?? "-")
                                                        .FontSize(11).FontColor("#374151");
                                                });

                                                row.RelativeItem(1).Column(c =>
                                                {
                                                    c.Item().Text($"{medicine.Quantity} viên")
                                                        .FontSize(11).FontColor("#374151");
                                                });

                                                row.RelativeItem(2).Column(c =>
                                                {
                                                    c.Item().Text(medicine.Instruction ?? "-")
                                                        .FontSize(10).FontColor("#6B7280");
                                                });
                                            });
                                    }
                                }
                            });
                    });

                    // ================= FOOTER =================
                    page.Footer()
                        .BorderTop(1)
                        .BorderColor("#E5E7EB")
                        .PaddingTop(12)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(col =>
                                {
                                    col.Item().Text("Tâm Anh Hospital - Chăm sóc sức khỏe toàn diện")
                                        .FontSize(10).FontColor("#9CA3AF");
                                    col.Item().PaddingTop(2)
                                        .Text($"In lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                                        .FontSize(9).FontColor("#D1D5DB");
                                });

                            row.ConstantItem(60)
                                .AlignRight()
                                .Column(col =>
                                {
                                    col.Item().Text("Trang {page}")
                                        .FontSize(10).FontColor("#9CA3AF");
                                });
                        });
                });
            });

            // Generate PDF
            var pdfBytes = document.GeneratePdf();

            // Open SaveFileDialog
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"DonThuoc_{prescription.PatientName}_{prescription.Date:yyyyMMdd_HHmmss}.pdf",
                DefaultExt = ".pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllBytes(saveFileDialog.FileName, pdfBytes);
                System.Windows.MessageBox.Show($"Đơn thuốc đã được in thành công!\n\nFile: {saveFileDialog.FileName}", 
                    "Thành công", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
    }
}
