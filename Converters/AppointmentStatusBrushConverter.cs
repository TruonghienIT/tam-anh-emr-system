using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TamAnh_EMR_System.Converters
{
    public class AppointmentStatusBrushConverter
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
                        (Color)ColorConverter.ConvertFromString("#FEF3C7")),

                "Đã xác nhận" =>
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#DBEAFE")),

                "Đang khám" =>
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#E9D5FF")),

                "Hoàn thành" =>
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#DCFCE7")),

                "Đã hủy" =>
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#FEE2E2")),

                _ =>
                    Brushes.LightGray
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