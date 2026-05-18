using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Text.Json;
using QuestPDF.Infrastructure;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Helpers;

namespace TamAnh_EMR_System.Services.Pdf
{
    public class AppointmentPdfService
    {
        public void Export(AppointmentDisplay appointment)
        {
            var qrData = new
            {
                appointmentId = appointment.Id,
                patientName = appointment.PatientName,
                phoneNumber = appointment.PhoneNumber,
                doctorName = appointment.DoctorName,
                department = appointment.Department,
                appointmentDate = appointment.AppointmentDate.ToString("dd/MM/yyyy"),
                appointmentTime = appointment.AppointmentTime.ToString(),
                status = appointment.Status,
                reason = appointment.Reason
            };

            string qrText = JsonSerializer.Serialize(qrData);

            byte[] qrBytes =
                QrCodeHelper.Generate(qrText);

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
                                .Text("PHIẾU LỊCH HẸN KHÁM BỆNH")
                                .SemiBold()
                                .FontSize(14)
                                .FontColor("#6B7280");
                        });

                        row.ConstantItem(120)
                            .AlignRight()
                            .Text($"#{appointment.Id}")
                            .Bold()
                            .FontSize(18)
                            .FontColor("#9CA3AF");
                    });

                    // ================= CONTENT =================

                    page.Content().PaddingVertical(24).Column(col =>
                    {
                        // CARD INFO

                        col.Item()
                            .Background(Colors.White)
                            .Border(1)
                            .BorderColor("#E5E7EB")
                            .CornerRadius(16)
                            .Padding(24)
                            .Column(info =>
                            {
                                info.Item()
                                    .Text("Thông tin cuộc hẹn")
                                    .Bold()
                                    .FontSize(18)
                                    .FontColor("#111827");

                                info.Item().PaddingTop(20);

                                info.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    void AddRow(
                                        string label,
                                        string value)
                                    {
                                        table.Cell()
                                            .PaddingBottom(14)
                                            .Text(label)
                                            .SemiBold()
                                            .FontColor("#6B7280");

                                        table.Cell()
                                            .PaddingBottom(14)
                                            .Text(value)
                                            .FontColor("#111827");
                                    }

                                    AddRow(
                                        "Bệnh nhân",
                                        appointment.PatientName);

                                    AddRow(
                                        "Số điện thoại",
                                        appointment.PhoneNumber);

                                    AddRow(
                                        "Bác sĩ",
                                        appointment.DoctorName);

                                    AddRow(
                                        "Khoa",
                                        appointment.Department);

                                    AddRow(
                                        "Ngày khám",
                                        appointment.AppointmentDate
                                            .ToString("dd/MM/yyyy"));

                                    AddRow(
                                        "Giờ khám",
                                        appointment.AppointmentTime);

                                    AddRow(
                                        "Trạng thái",
                                        appointment.Status);
                                });
                            });

                        // TRIỆU CHỨNG

                        col.Item().PaddingTop(20);

                        col.Item()
                            .Background(Colors.White)
                            .Border(1)
                            .BorderColor("#E5E7EB")
                            .CornerRadius(16)
                            .Padding(24)
                            .Column(symptom =>
                            {
                                symptom.Item()
                                    .Text("Triệu chứng")
                                    .Bold()
                                    .FontSize(16);

                                symptom.Item()
                                    .PaddingTop(12)
                                    .Text(string.IsNullOrWhiteSpace(
                                        appointment.Reason)
                                        ? "Không có mô tả"
                                        : appointment.Reason)
                                    .FontColor("#374151");
                            });

                        // STATUS BADGE
                        col.Item().PaddingTop(20).Row(row =>
                        {
                            row.ConstantItem(100)
                                .Text("Trạng thái:")
                                .Bold()
                                .FontColor("#6B7280");

                            row.RelativeItem()
                                .Text(appointment.Status)
                                .Bold()
                                .FontColor(GetStatusText(appointment.Status));
                        });

                        // QR

                        col.Item().PaddingTop(40);

                        col.Item()
                            .AlignCenter()
                            .Column(qr =>
                            {
                                qr.Item()
                                    .Text("QR THÔNG TIN LỊCH HẸN")
                                    .Bold()
                                    .FontSize(14);

                                qr.Item()
                                    .PaddingTop(16)
                                    .Width(160)
                                    .Height(160)
                                    .Image(qrBytes);
                            });
                    });

                    // ================= FOOTER =================

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(
                                "Tâm Anh Hospital • Hệ thống quản lý bệnh án điện tử")
                                .FontSize(10)
                                .FontColor("#9CA3AF");
                        });
                });
            });

            var saveDialog = new SaveFileDialog
            {
                FileName =
                    $"Appointment_{appointment.Id}",

                DefaultExt = ".pdf",

                Filter =
                    "PDF files (*.pdf)|*.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                document.GeneratePdf(
                    saveDialog.FileName);
            }
        }

        // ================= STATUS COLOR =================

        private string GetStatusBg(string status)
        {
            return status switch
            {
                "Đang chờ" => "#FEF3C7",
                "Hoàn thành" => "#DCFCE7",
                "Đã hủy" => "#FEE2E2",
                _ => "#E5E7EB"
            };
        }

        private string GetStatusText(string status)
        {
            return status switch
            {
                "Đang chờ" => "#D97706",
                "Hoàn thành" => "#16A34A",
                "Đã hủy" => "#DC2626",
                _ => "#374151"
            };
        }
    }
}