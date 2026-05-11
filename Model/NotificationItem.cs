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
                    _ => Brushes.DodgerBlue
                };
            }
        }
    }
}