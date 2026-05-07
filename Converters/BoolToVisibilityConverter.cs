using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TamAnh_EMR_System.Converters
{
    /// <summary>
    /// Standard boolean-to-Visibility converter for showing/hiding UI elements.
    /// true  → Visibility.Visible
    /// false → Visibility.Collapsed
    /// 
    /// Pass "Inverse" as ConverterParameter to reverse the logic:
    /// true  → Collapsed
    /// false → Visible
    /// 
    /// XAML usage:
    ///   Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;

            // Support inverse mode via parameter
            if (parameter is string param && param == "Inverse")
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
