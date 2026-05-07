using System.Windows;
using TamAnh_EMR_System.ViewModel;

namespace TamAnh_EMR_System.View
{
    /// <summary>
    /// Code-behind for ReceptionistView (Dashboard).
    /// 
    /// ONLY responsibility: set DataContext to the ViewModel.
    /// All business logic, data, and commands live in ReceptionistDashboardViewModel.
    /// All child components inherit this DataContext automatically via WPF's 
    /// DataContext inheritance mechanism.
    /// </summary>
    public partial class ReceptionistView : Window
    {
        public ReceptionistView()
        {
            InitializeComponent();

            // Set the ViewModel as the DataContext for the entire view tree.
            // All child UserControls (Sidebar, Header, Table, etc.) will
            // automatically inherit this DataContext through WPF's visual tree.
            DataContext = new ReceptionistDashboardViewModel();
        }
    }
}
