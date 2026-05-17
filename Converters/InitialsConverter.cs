using System;
using System.Globalization;
using System.Windows.Data;

namespace TamAnh_EMR_System.Converters
{
    public class InitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "?";
            var s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return "?";
            var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            }

            var first = parts[0][0].ToString();
            var last = parts[parts.Length - 1][0].ToString();
            return (first + last).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
