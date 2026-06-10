using DotNetEnv;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.Services
{
    public class TelegramService
    {
        static TelegramService()
        {
            Env.Load();
        }

        public static async Task SendMessageAsync(
            string chatId,
            string doctorName,
            string patientName,
            string patientPhone,
            string appointmentId,
            DateTime appointmentDate,
            string appointmentTime,
            string reason)
        {
            string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

            string message =
                "🏥 TAM ANH EMR SYSTEM\n\n" +
                "📢 CÓ LỊCH HẸN KHÁM MỚI\n\n" +
                $"👨‍⚕️ Bác sĩ: {doctorName}\n" +
                $"🆔 Mã lịch hẹn: {appointmentId}\n" +
                $"👤 Bệnh nhân: {patientName}\n" +
                $"📞 SĐT: {patientPhone}\n" +
                $"📅 Ngày khám: {appointmentDate:dd/MM/yyyy}\n" +
                $"⏰ Giờ khám: {appointmentTime}\n" +
                $"🩺 Triệu chứng/Lý do khám: {reason}\n\n" +
                "Vui lòng kiểm tra lịch làm việc.";


            using HttpClient client = new HttpClient();

            string url = $"https://api.telegram.org/bot{botToken}/sendMessage";

            var content =
                new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string,string>("chat_id", chatId),
                    new KeyValuePair<string,string>("text", message)
                });

            await client.PostAsync(url, content);
        }
    }
}