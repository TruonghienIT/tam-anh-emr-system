using FontAwesome.Sharp;

namespace TamAnh_EMR_System.Model
{
    /// <summary>
    /// Represents a navigation item in the sidebar menu.
    /// Each item has an icon (FontAwesome), display title, and active state.
    /// 
    /// Bound to an ItemsControl in SidebarControl.xaml.
    /// The IsSelected property drives visual highlighting of the active page.
    /// </summary>
    public class SidebarMenuItem
    {
        /// <summary>Display text (e.g., "Tổng quan", "Lịch hẹn")</summary>
        public string Title { get; set; }

        /// <summary>FontAwesome icon identifier for the menu item</summary>
        public IconChar Icon { get; set; }

        /// <summary>Whether this menu item is currently active/selected</summary>
        public bool IsSelected { get; set; }
    }
}
