using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Windows;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Services.Pdf
{
    public class MedicalRecordPdfService
    {
        public void Export(MedicalRecords record)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor("#F8FAFC");
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor("#1F2937").FontFamily(Fonts.Arial));

                    // ================= HEADER =================
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
                                    .Text("HỒ SƠ BỆNH ÁN CHI TIẾT")
                                    .SemiBold()
                                    .FontSize(14)
                                    .FontColor("#4B5563");
                            });

                            row.ConstantItem(150)
                                .AlignRight()
                                .Text($"Mã BA: #{record.Id}")
                                .SemiBold()
                                .FontSize(14)
                                .FontColor("#9CA3AF");
                        });

                        header.Item().PaddingTop(15).LineHorizontal(1).LineColor("#E5E7EB");
                    });

                    // ================= CONTENT =================
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // 1. THÔNG TIN CHUNG (ĐÃ CHIA NHÓM RÕ RÀNG)
                        col.Item()
                            .Background(Colors.White)
                            .Border(1)
                            .BorderColor("#E5E7EB")
                            .CornerRadius(8)
                            .Padding(20)
                            .Column(info =>
                            {
                                info.Item().Text("1. THÔNG TIN CHUNG").Bold().FontSize(14).FontColor("#111827");
                                info.Item().PaddingTop(15);
                                info.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.2f); // Nhãn 1
                                        columns.RelativeColumn(2f);   // Giá trị 1
                                        columns.RelativeColumn(1.2f); // Nhãn 2
                                        columns.RelativeColumn(2f);   // Giá trị 2
                                    });

                                    void AddCellLabel(string text) => table.Cell().PaddingBottom(10).Text(text).SemiBold().FontColor("#6B7280");
                                    void AddCellValue(string text) => table.Cell().PaddingBottom(10).Text(text).FontColor("#111827");

                                    string dobString = "N/A";
                                    if (record.Patient?.Dob != null && record.Patient.Dob != DateTime.MinValue)
                                    {
                                        dobString = record.Patient.Dob.ToString("dd/MM/yyyy");
                                    }

                                    // --- NHÓM 1: THÔNG TIN LƯỢT KHÁM ---
                                    AddCellLabel("Mã bệnh án:");
                                    AddCellValue(record.Id);
                                    AddCellLabel("Ngày khám:");
                                    AddCellValue(record.CreatedAt.ToString("dd/MM/yyyy HH:mm"));

                                    AddCellLabel("Bác sĩ khám:");
                                    AddCellValue(record.Doctor?.FullName ?? "N/A");
                                    table.Cell(); table.Cell(); // Bỏ trống để phần bên phải thoáng hơn

                                    // Đường kẻ mờ phân cách 2 nhóm
                                    table.Cell().ColumnSpan(4).PaddingVertical(6).LineHorizontal(1).LineColor("#F3F4F6");

                                    // --- NHÓM 2: THÔNG TIN BỆNH NHÂN ---
                                    // PaddingTop một chút để cách xa đường kẻ
                                    table.Cell().PaddingTop(10).PaddingBottom(10).Text("Bệnh nhân:").SemiBold().FontColor("#6B7280");
                                    table.Cell().PaddingTop(10).PaddingBottom(10).Text(record.Patient?.Name ?? "N/A").FontColor("#111827");

                                    table.Cell().PaddingTop(10).PaddingBottom(10).Text("Giới tính:").SemiBold().FontColor("#6B7280");
                                    table.Cell().PaddingTop(10).PaddingBottom(10).Text(record.Patient?.Gender ?? "N/A").FontColor("#111827");

                                    AddCellLabel("Ngày sinh:");
                                    AddCellValue(dobString);
                                    AddCellLabel("Số điện thoại:");
                                    AddCellValue(record.Patient?.Phone ?? "N/A"); // Đổi thành PhoneNumber nếu Model của bạn tên khác

                                    AddCellLabel("Địa chỉ:");
                                    // Cho phép Địa chỉ chiếm trọn 3 cột còn lại để hiển thị dài thoải mái
                                    table.Cell().ColumnSpan(3).PaddingBottom(10).Text(record.Patient?.Address ?? "N/A").FontColor("#111827");
                                });
                            });

                        // 2. CHỈ SỐ SINH TỒN
                        col.Item().PaddingTop(15);
                        col.Item()
                            .Background(Colors.White)
                            .Border(1)
                            .BorderColor("#E5E7EB")
                            .CornerRadius(8)
                            .Padding(20)
                            .Column(vitals =>
                            {
                                vitals.Item().Text("2. CHỈ SỐ SINH TỒN").Bold().FontSize(14).FontColor("#2563EB");

                                vitals.Item().PaddingTop(15).Row(row =>
                                {
                                    void AddVitalBox(RowDescriptor r, string label, string value, string unit)
                                    {
                                        r.RelativeItem().PaddingRight(10).Background("#EFF6FF").CornerRadius(6).Padding(12).Column(c =>
                                        {
                                            c.Item().Text(label).FontSize(11).FontColor("#6B7280");
                                            c.Item().PaddingTop(4).Text($"{value} {unit}").Bold().FontSize(15).FontColor("#1E3A8A");
                                        });
                                    }

                                    AddVitalBox(row, "Nhịp tim", record.Pulse, "bpm");
                                    AddVitalBox(row, "Huyết áp", record.BloodPressure, "mmHg");
                                    AddVitalBox(row, "Nhiệt độ", record.Temperature, "°C");
                                    AddVitalBox(row, "SpO2", record.SPO2, "%");
                                });
                            });

                        // 3. XÉT NGHIỆM LÂM SÀNG
                        var lab = record.LabResults?.FirstOrDefault();
                        if (lab != null && !string.IsNullOrWhiteSpace(lab.TestName))
                        {
                            col.Item().PaddingTop(15);
                            col.Item()
                                .Background(Colors.White)
                                .Border(1)
                                .BorderColor("#E5E7EB")
                                .CornerRadius(8)
                                .Padding(20)
                                .Column(labs =>
                                {
                                    labs.Item().Text("3. XÉT NGHIỆM LÂM SÀNG").Bold().FontSize(14).FontColor("#2563EB");

                                    labs.Item().PaddingTop(15).Text("Tên xét nghiệm:").SemiBold().FontColor("#6B7280");
                                    labs.Item().PaddingBottom(10).Text(lab.TestName).FontColor("#111827");

                                    labs.Item().Text("Kết quả:").SemiBold().FontColor("#6B7280");
                                    labs.Item().Text(lab.Result).FontColor("#111827");
                                });
                        }

                        // 4. THÔNG TIN LÂM SÀNG
                        col.Item().PaddingTop(15);
                        col.Item()
                            .Background(Colors.White)
                            .Border(1)
                            .BorderColor("#E5E7EB")
                            .CornerRadius(8)
                            .Padding(20)
                            .Column(clinical =>
                            {
                                clinical.Item().Text("4. THÔNG TIN LÂM SÀNG").Bold().FontSize(14).FontColor("#2563EB");

                                void AddClinicalRow(ColumnDescriptor c, string label, string value)
                                {
                                    c.Item().PaddingTop(15).Text(label).SemiBold().FontColor("#6B7280");
                                    c.Item().PaddingTop(4).Text(string.IsNullOrWhiteSpace(value) ? "Không có" : value).FontColor("#111827");
                                }

                                AddClinicalRow(clinical, $"Chẩn đoán (ICD: {record.IcdCode}):", record.Diagnosis);
                                AddClinicalRow(clinical, "Phương pháp điều trị:", record.Treatment);
                                AddClinicalRow(clinical, "Ghi chú / Lời dặn của bác sĩ:", record.Notes);
                            });

                        // =========================================================
                        // 5. ĐƠN THUỐC
                        // =========================================================
                        col.Item().PaddingTop(15);
                        col.Item()
                            .Background(Colors.White)
                            .Border(1)
                            .BorderColor("#E5E7EB")
                            .CornerRadius(8)
                            .Padding(20)
                            .Column(prescription =>
                            {
                                prescription.Item().Text("5. ĐƠN THUỐC").Bold().FontSize(14).FontColor("#2563EB");
                                prescription.Item().PaddingTop(15);

                                // Kiểm tra xem có đơn thuốc không
                                if (record.PrescriptionDetails != null && record.PrescriptionDetails.Any())
                                {
                                    prescription.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2); // Tên thuốc
                                            columns.RelativeColumn(1); // SL
                                            columns.RelativeColumn(2); // Cách dùng
                                            columns.RelativeColumn(2); // Ghi chú
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Padding(5).Text("Tên thuốc").SemiBold().FontSize(10).FontColor("#6B7280");
                                            header.Cell().Padding(5).Text("SL").SemiBold().FontSize(10).FontColor("#6B7280");
                                            header.Cell().Padding(5).Text("Cách dùng").SemiBold().FontSize(10).FontColor("#6B7280");
                                            header.Cell().Padding(5).Text("Ghi chú").SemiBold().FontSize(10).FontColor("#6B7280");
                                        });

                                        foreach (var item in record.PrescriptionDetails)
                                        {
                                            table.Cell().Padding(5).BorderBottom(0.5f).BorderColor("#E5E7EB").Text(item.MedicineName ?? "N/A");
                                            table.Cell().Padding(5).BorderBottom(0.5f).BorderColor("#E5E7EB").Text(item.Quantity.ToString());
                                            table.Cell().Padding(5).BorderBottom(0.5f).BorderColor("#E5E7EB").Text(item.Frequency ?? "");
                                            table.Cell().Padding(5).BorderBottom(0.5f).BorderColor("#E5E7EB").Text(item.Notes ?? "");
                                        }
                                    });
                                }
                                else
                                {
                                    // Nếu không có đơn thuốc -> Hiển thị thông báo
                                    prescription.Item()
                                        .Padding(10)
                                        .AlignCenter()
                                        .Text("Bệnh nhân không có đơn thuốc trong lượt khám này.")
                                        .FontSize(11)
                                        .FontColor("#9CA3AF")
                                        .Italic();
                                }
                            });
                    });


                    // ================= FOOTER =================
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Phòng khám Đa khoa Tâm Anh • Hệ thống quản lý bệnh án điện tử")
                                .FontSize(10)
                                .FontColor("#9CA3AF");
                        });
                });
            });

            var saveDialog = new SaveFileDialog
            {
                FileName = $"BenhAn_{record.Patient?.Name ?? "BN"}_{record.Id}",
                DefaultExt = ".pdf",
                Filter = "PDF files (*.pdf)|*.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                document.GeneratePdf(saveDialog.FileName);
                MessageBox.Show("Đã xuất file PDF thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}