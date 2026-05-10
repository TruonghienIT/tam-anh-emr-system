using System.Windows.Controls;
using TamAnh_EMR_System.ViewModel.Doctor; // Nhớ using đúng Namespace

namespace TamAnh_EMR_System.View.Doctor
{
    public partial class DoctorPatientManagementView : UserControl
    {
        public DoctorPatientManagementView()
        {
            InitializeComponent();
            this.DataContext = new DoctorPatientManagementViewModel();
        }
    }
}