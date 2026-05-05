using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TamAnh_EMR_System.Converters
{
    /// <summary>
    /// Converts appointment status text to its corresponding foreground color.
    /// Used in the appointment table to color-code the status badge text.
    /// 
    /// Status mapping (matching the design):
    ///   "Đang khám"  → Blue   (#3B82F6)
    ///   "Đang chờ"   → Orange (#F59E0B)
    ///   "Hoàn thành" → Green  (#10B981)
    ///   "Đã hủy"     → Red    (#EF4444)
    /// 
    /// XAML usage:
    ///   Foreground="{Binding Status, Converter={StaticResource StatusToColorConverter}}"
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;

            return status switch
            {
                "Đang khám" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
                "Đang chờ" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                "Hoàn thành" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                "Đã hủy" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
