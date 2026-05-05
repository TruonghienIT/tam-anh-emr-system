using System.Windows.Controls;

namespace TamAnh_EMR_System.View.Components
{
    /// <summary>
    /// Code-behind for SidebarControl.
    /// DataContext is inherited from the parent ReceptionistView (set to ReceptionistDashboardViewModel).
    /// No business logic here — only component initialization.
    /// </summary>
    public partial class SidebarControl : UserControl
    {
        public SidebarControl()
        {
            InitializeComponent();
        }
    }
}
