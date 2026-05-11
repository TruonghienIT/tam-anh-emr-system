using System;
using System.Globalization;
using System.Windows.Data;

namespace TamAnh_EMR_System.Converters
{
    public class PrevPageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int currentPage = System.Convert.ToInt32(value);

            return currentPage <= 1
                ? 1
                : currentPage - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}