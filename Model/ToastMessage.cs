using System.Windows.Media;

namespace TamAnh_EMR_System.Model
{
    public class ToastMessage
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public Brush Background { get; set; }

        public Brush BorderBrush { get; set; }
    }
}