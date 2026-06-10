using DotNetEnv;
using System;
using TamAnh_EMR_System.Model.Doctor;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TamAnh_EMR_System.Services
{
    public class SmsService
    {
        static SmsService()
        {
            Env.Load();
        }

        public static void SendSms(
            string toPhone,
            string patientName,
            DateTime appointmentDate,
            string appointmentTime)
        {
            string sid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");

            string token = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

            string fromPhone = Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER");


            if (string.IsNullOrWhiteSpace(sid))
                throw new Exception("Missing TWILIO_ACCOUNT_SID");

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Missing TWILIO_AUTH_TOKEN");

            if (string.IsNullOrWhiteSpace(fromPhone))
                throw new Exception("Missing TWILIO_PHONE_NUMBER");

            TwilioClient.Init(sid, token);

            var message = MessageResource.Create(
                body:
                    $"Xin chào {patientName}, " +
                    $"bạn đã đăng ký lịch khám thành công vào " +
                    $"{appointmentDate:dd/MM/yyyy} lúc {appointmentTime}.",

                from: new PhoneNumber(fromPhone),

                to: new PhoneNumber(FormatPhoneNumber(toPhone))
            );

            System.Diagnostics.Debug.WriteLine(
                $"SMS SENT: {message.Sid}");
        }

        private static string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new Exception("Phone number is empty");

            phone = phone.Trim();

            if (phone.StartsWith("0"))
                phone = "+84" + phone.Substring(1);

            return phone;
        }
    }
}