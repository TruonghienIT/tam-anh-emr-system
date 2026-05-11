using System.Windows;
using TamAnh_EMR_System.ViewModel;

namespace TamAnh_EMR_System.View
{
    public partial class ReceptionistView : Window
    {
        public ReceptionistView()
        {
            InitializeComponent();
            DataContext = new ReceptionistDashboardViewModel();
        }
    }
}
