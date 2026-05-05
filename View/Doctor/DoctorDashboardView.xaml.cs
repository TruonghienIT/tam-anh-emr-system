using System.Windows.Controls;
using TamAnh_EMR_System.ViewModel.Doctor;

namespace TamAnh_EMR_System.View.Doctor
{
    public partial class DoctorDashboardView : UserControl
    {
        public DoctorDashboardView()
        {
            InitializeComponent();
            DataContext = new DoctorDashboardViewModel();
        }
    }
}
