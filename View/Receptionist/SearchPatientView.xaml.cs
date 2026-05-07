using System.Windows.Controls;
using TamAnh_EMR_System.ViewModel.Receptionist;

namespace TamAnh_EMR_System.View.Receptionist
{
    public partial class SearchPatientView : UserControl
    {
        public SearchPatientView()
        {
            InitializeComponent();
            DataContext = new SearchPatientViewModel();
        }
    }
}
