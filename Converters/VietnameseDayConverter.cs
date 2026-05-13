using System;
using System.Globalization;
using System.Windows.Data;

namespace TamAnh_EMR_System.Converters
{
    public class VietnameseDayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        return "Thứ Hai";

                    case DayOfWeek.Tuesday:
                        return "Thứ Ba";

                    case DayOfWeek.Wednesday:
                        return "Thứ Tư";

                    case DayOfWeek.Thursday:
                        return "Thứ Năm";

                    case DayOfWeek.Friday:
                        return "Thứ Sáu";

                    case DayOfWeek.Saturday:
                        return "Thứ Bảy";

                    case DayOfWeek.Sunday:
                        return "Chủ Nhật";
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}