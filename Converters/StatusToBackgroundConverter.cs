using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TamAnh_EMR_System.Converters
{
    public class StatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value?.ToString()?.ToLower();

            return status switch
            {
                "đang khám" => new SolidColorBrush(Color.FromRgb(220, 235, 255)),
                "đang chờ" => new SolidColorBrush(Color.FromRgb(255, 240, 220)),
                "hoàn thành" => new SolidColorBrush(Color.FromRgb(220, 255, 230)),
                "đã hủy" => new SolidColorBrush(Color.FromRgb(255, 220, 220)),
                _ => new SolidColorBrush(Color.FromRgb(240, 240, 240))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}