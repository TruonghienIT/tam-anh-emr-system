using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TamAnh_EMR_System.Converters
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            // =====================================================
            // BOOLEAN
            // =====================================================

            if (value is bool boolValue)
            {
                return boolValue
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

            // =====================================================
            // INTEGER (Patients.Count)
            // =====================================================

            if (value is int intValue)
            {
                return intValue == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }
}