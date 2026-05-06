using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Services
{
    public class EmailService
    {
        private readonly string _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
        private readonly int _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT"));

        private readonly string _fromEmail = Environment.GetEnvironmentVariable("MAIL_NAME");
        private readonly string _fromPassword = Environment.GetEnvironmentVariable("MAIL_PASSWORD");

        public async Task SendAccountEmailAsync(string toEmail, string username, string password, string role)
        {
            try
            {
                string roleDisplay = GetRoleDisplayName(role);

                var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, "TamAnh EMR System");
                message.To.Add(toEmail);
                message.Subject = $"Thông tin tài khoản {roleDisplay}";
                message.Body = $@" Xin chào, Tài khoản {roleDisplay} của bạn đã được tạo thành công:

                Username: {username}
                Password: {password}

        Vui lòng đăng nhập và đổi mật khẩu sau khi đăng nhập.

                Trân trọng!
                Hệ thống TamAnh EMR";

                message.IsBodyHtml = false;

                using (var smtp = new SmtpClient(_smtpHost, _smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Gửi email thất bại: " + ex.Message);
            }
        }

        public async Task SendResetPasswordEmailAsync(string toEmail, string username, string newPassword)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, "TamAnh EMR System");
                message.To.Add(toEmail);
                message.Subject = "Đặt lại mật khẩu";

                message.Body = $@"Xin chào, Bạn đã yêu cầu đặt lại mật khẩu.
Thông tin đăng nhập mới:
Username: {username}
Password: {newPassword}

Vui lòng đăng nhập và đổi mật khẩu ngay sau khi đăng nhập.

Trân trọng!
Hệ thống TamAnh EMR
";

                message.IsBodyHtml = false;

                using (var smtp = new SmtpClient(_smtpHost, _smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Gửi email reset thất bại: " + ex.Message);
            }
        }

        private string GetRoleDisplayName(string role)
        {
            return role switch
            {
                "doctor" => "bác sĩ",
                "receptionist" => "lễ tân",
                "admin" => "quản trị viên",
                _ => role
            };
        }
    }
}