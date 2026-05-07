using System.Windows.Controls;

namespace TamAnh_EMR_System.View.Components
{
    /// <summary>
    /// Code-behind for DashboardContentControl.
    /// 
    /// This is a pure container — no logic, no DataContext override.
    /// DataContext is inherited from the parent ReceptionistView 
    /// (ReceptionistDashboardViewModel), so all bindings resolve correctly.
    /// </summary>
    public partial class DashboardContentControl : UserControl
    {
        public DashboardContentControl()
        {
            InitializeComponent();
            // DataContext inherited from parent — DO NOT set here
        }
    }
}
