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

        public async Task SendAccountEmailAsync(string toEmail, string username, string password)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, "TamAnh EMR System");
                message.To.Add(toEmail);
                message.Subject = "Thông tin tài khoản bác sĩ";
                message.Body = $@" Xin chào, Tài khoản bác sĩ của bạn đã được tạo thành công:

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
    }
}