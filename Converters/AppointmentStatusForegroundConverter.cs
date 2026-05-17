using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TamAnh_EMR_System.Converters
{
    public class AppointmentStatusForegroundConverter
        : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            string status =
                value?.ToString();

            return status switch
            {
                "Đang chờ" =>
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#D97706")),

                "Đã xác nhận" =>
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#2563EB")),

                "Đang khám" =>
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#9333EA")),

                "Hoàn thành" =>
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#16A34A")),

                "Đã hủy" =>
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#DC2626")),

                _ =>
                    Brushes.Black
            };
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}