using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TamAnh_EMR_System.Model.Doctor;

namespace TamAnh_EMR_System.Services.Pdf
{
    public class PrescriptionPdfService
    {
        public void Export(
    Prescription prescription,
    List<MedicineItem> medicines)
{
    QuestPDF.Settings.License = LicenseType.Community;

    var document = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);

            page.PageColor("#F3F6FB");

            page.DefaultTextStyle(x =>
                x.FontSize(11)
                 .FontFamily(Fonts.Arial)
                 .FontColor("#1F2937"));

            // =====================================================
            // HEADER
            // =====================================================

            page.Header().Column(header =>
            {
                header.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item()
                            .Text("PHÒNG KHÁM ĐA KHOA TÂM ANH")
                            .Bold()
                            .FontSize(20)
                            .FontColor("#2563EB");

                        col.Item()
                            .PaddingTop(4)
                            .Text("ĐƠN THUỐC")
                            .SemiBold()
                            .FontSize(14)
                            .FontColor("#4B5563");
                    });

                    row.ConstantItem(140)
                        .AlignRight()
                        .Column(col =>
                        {
                            col.Item()
                                .AlignRight()
                                .Text($"#{prescription.RecordId}")
                                .Bold()
                                .FontSize(18)
                                .FontColor("#2563EB");

                            col.Item()
                                .PaddingTop(5)
                                .AlignRight()
                                .Text(prescription.Date.ToString("dd/MM/yyyy"))
                                .FontSize(11)
                                .FontColor("#4B5563");
                        });
                });
                header.Item()
                    .PaddingTop(15)
                    .LineHorizontal(1)
                    .LineColor("#E5E7EB");
            });


            // =====================================================
            // CONTENT
            // =====================================================
            page.Content().Column(content =>
            {
                // =================================================
                // PATIENT INFO
                // =================================================
                content.Item()
                    .Background(Colors.White)
                    .Border(1)
                    .BorderColor("#E5E7EB")
                    .CornerRadius(14)
                    .Padding(24)
                    .Column(info =>
                    {
                        info.Item()
                            .Text("THÔNG TIN BỆNH NHÂN")
                            .Bold()
                            .FontSize(15)
                            .FontColor("#111827");

                        info.Item().PaddingTop(18);

                        info.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(2f);
                            });

                            void Label(string text) =>
                                table.Cell()
                                    .PaddingBottom(14)
                                    .Text(text)
                                    .SemiBold()
                                    .FontColor("#6B7280");

                            void Value(string text) =>
                                table.Cell()
                                    .PaddingBottom(14)
                                    .Text(text)
                                    .FontColor("#111827");

                            Label("Mã bệnh nhân");
                            Value(prescription.PatientId);

                            Label("Ngày kê");
                            Value(prescription.Date.ToString("dd/MM/yyyy HH:mm"));

                            Label("Bệnh nhân");
                            Value(prescription.PatientName);

                            Label("Bác sĩ điều trị");
                            Value(prescription.DoctorName);
                        });
                    });

                // =================================================
                // MEDICINE TABLE
                // =================================================
                content.Item().PaddingTop(20);

                content.Item()
                    .Background(Colors.White)
                    .Border(1)
                    .BorderColor("#E5E7EB")
                    .CornerRadius(14)
                    .Padding(24)
                    .Column(medicine =>
                    {
                        medicine.Item()
                            .Text("DANH SÁCH THUỐC")
                            .Bold()
                            .FontSize(15)
                            .FontColor("#111827");

                        medicine.Item().PaddingTop(18);

                        if (medicines != null && medicines.Any())
                        {
                            medicine.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2.2f);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(2.5f);
                                    columns.RelativeColumn(0.8f);
                                });

                                // HEADER
                                table.Header(header =>
                                {
                                    void HeaderCell(string text)
                                    {
                                        header.Cell()
                                            .PaddingVertical(10)
                                            .PaddingHorizontal(8)
                                            .Text(text)
                                            .SemiBold()
                                            .FontSize(11)
                                            .FontColor("#1D4ED8");
                                    }

                                    HeaderCell("Tên thuốc");
                                    HeaderCell("Liều dùng");
                                    HeaderCell("Hướng dẫn");
                                    HeaderCell("SL");
                                });

                                // ROWS
                                foreach (var item in medicines)
                                {
                                    void BodyCell(string text)
                                    {
                                        table.Cell()
                                            .PaddingVertical(10)
                                            .PaddingHorizontal(8)
                                            .BorderBottom(0.5f)
                                            .BorderColor("#E5E7EB")
                                            .Text(text)
                                            .FontColor("#374151");
                                    }

                                    BodyCell(item.Name);

                                    BodyCell(
                                        string.IsNullOrWhiteSpace(item.Dosage)
                                        ? "Không có"
                                        : item.Dosage);

                                    BodyCell(
                                        string.IsNullOrWhiteSpace(item.Instruction)
                                        ? "Không có"
                                        : item.Instruction);

                                    BodyCell(item.Quantity.ToString());
                                }
                            });
                        }
                        else
                        {
                            medicine.Item()
                                .Padding(20)
                                .AlignCenter()
                                .Text("Không có thuốc trong đơn")
                                .Italic()
                                .FontColor("#9CA3AF");
                        }
                    });

                // =================================================
                // NOTE
                // =================================================
                content.Item().PaddingTop(20);

                content.Item()
                    .Background("#EFF6FF")
                    .Border(1)
                    .BorderColor("#BFDBFE")
                    .CornerRadius(12)
                    .Padding(16)
                    .Column(note =>
                    {
                        note.Item()
                            .Text("LƯU Ý")
                            .Bold()
                            .FontSize(13)
                            .FontColor("#1D4ED8");

                        note.Item()
                            .PaddingTop(8)
                            .Text("• Uống thuốc đúng liều lượng được kê.\n" +
                                  "• Không tự ý ngưng thuốc.\n" +
                                  "• Đi khám lại nếu có dấu hiệu bất thường.")
                            .FontSize(11)
                            .FontColor("#374151");
                    });

                // =================================================
                // SIGNATURE
                // =================================================
                content.Item().PaddingTop(40);

                content.Item().AlignRight().Width(220)
                    .Column(sign =>
                    {
                        sign.Item()
                            .AlignCenter()
                            .Text("Bác sĩ điều trị")
                            .SemiBold()
                            .FontColor("#6B7280");

                        sign.Item()
                            .PaddingTop(60)
                            .AlignCenter()
                            .Text(prescription.DoctorName)
                            .Bold()
                            .FontSize(13)
                            .FontColor("#111827");
                    });
            });

            // =====================================================
            // FOOTER
            // =====================================================
            page.Footer().PaddingTop(10).AlignCenter().Text(text =>
            {
                text.Span("Phòng khám Đa khoa Tâm Anh • Hệ thống EMR")
                    .FontSize(10)
                    .FontColor("#9CA3AF");
            });
        });
    });

    var saveDialog = new SaveFileDialog
    {
        FileName = $"DonThuoc_{prescription.PatientName}_{prescription.RecordId}",
        DefaultExt = ".pdf",
        Filter = "PDF files (*.pdf)|*.pdf"
    };

    if (saveDialog.ShowDialog() == true)
    {
        document.GeneratePdf(saveDialog.FileName);

        MessageBox.Show(
            "Xuất đơn thuốc PDF thành công!",
            "Thành công",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
    }
}