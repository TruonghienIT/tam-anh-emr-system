using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TamAnh_EMR_System.Converters
{
    /// <summary>
    /// Converts appointment status text to a light background color for the status badge.
    /// Works in tandem with StatusToColorConverter (foreground) to create
    /// the colored pill/badge effect seen in the design.
    /// 
    /// Status mapping:
    ///   "Đang khám"  → Light Blue   (#EFF6FF)
    ///   "Đang chờ"   → Light Orange (#FFF7ED)
    ///   "Hoàn thành" → Light Green  (#ECFDF5)
    ///   "Đã hủy"     → Light Red    (#FEF2F2)
    /// 
    /// XAML usage:
    ///   Background="{Binding Status, Converter={StaticResource StatusToBackgroundConverter}}"
    /// </summary>
    public class StatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;

            return status switch
            {
                "Đang khám" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EFF6FF")),
                "Đang chờ" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF7ED")),
                "Hoàn thành" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECFDF5")),
                "Đã hủy" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF2F2")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F4F6")),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
