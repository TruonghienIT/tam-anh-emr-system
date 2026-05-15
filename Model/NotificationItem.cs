using FontAwesome.Sharp;
using System;
using System.Windows.Media;

namespace TamAnh_EMR_System.Model
{
    public class NotificationItem
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string TimeText { get; set; }

        public string Type { get; set; }

        public DateTime CreatedAt { get; set; }

        // =====================================================
        // UI COLOR FOR NOTIFICATION DOT
        // =====================================================

        public Brush DotColor
        {
            get
            {
                return Type switch
                {
                    "success" => Brushes.LimeGreen,
                    "error" => Brushes.IndianRed,
                    "warning" => Brushes.DarkOrange,
                    "info" => Brushes.MediumPurple,
                    _ => Brushes.DodgerBlue
                };
            }
        }

        // =====================================================
        // ICON
        // =====================================================

        public IconChar Icon
        {
            get
            {
                return Title switch
                {
                    "Thêm bệnh nhân mới" => IconChar.UserPlus,

                    "Đặt lịch khám" => IconChar.CalendarPlus,

                    "Tạo bệnh án mới" => IconChar.FileCirclePlus,

                    _ => IconChar.CircleInfo
                };
            }
        }
    }
}