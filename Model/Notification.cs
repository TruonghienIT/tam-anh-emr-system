using System;
using System.Windows.Media;

namespace TamAnh_EMR_System.Model
{
    /// <summary>
    /// Represents a notification item in the dashboard's "Thông báo mới" panel.
    /// Each notification has a colored dot indicator, title, description, and timestamp.
    /// 
    /// Bound to an ItemsControl in NotificationPanel.xaml via ObservableCollection.
    /// The DotColor property differentiates notification types visually.
    /// </summary>
    public class Notification
    {
        /// <summary>Notification headline (e.g., "Bệnh nhân mới đăng ký khám")</summary>
        public string Title { get; set; }

        /// <summary>Detailed description text</summary>
        public string Description { get; set; }

        /// <summary>When the notification occurred</summary>
        public DateTime Time { get; set; }

        /// <summary>Display-formatted time string (e.g., "10:15 sáng nay")</summary>
        public string TimeDisplay { get; set; }

        /// <summary>Color of the indicator dot (blue, orange, red, etc.)</summary>
        public Brush DotColor { get; set; }
    }
}
