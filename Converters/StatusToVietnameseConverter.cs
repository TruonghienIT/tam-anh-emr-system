using System;
using System.Globalization;
using System.Windows.Data;

namespace TamAnh_EMR_System.Converters
{
    public class StatusToVietnameseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Pending" => "Đang chờ",
                    "Confirmed" => "Đang khám",
                    "Completed" => "Hoàn thành",
                    "Cancelled" => "Đã hủy",
                    _ => status
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
