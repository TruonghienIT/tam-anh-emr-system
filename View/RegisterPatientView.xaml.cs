using System.Windows.Controls;
using TamAnh_EMR_System.ViewModel;

namespace TamAnh_EMR_System.View
{
    /// <summary>
    /// Code-behind for RegisterPatientView.
    /// Only sets DataContext — all logic is in RegisterPatientViewModel.
    /// </summary>
    public partial class RegisterPatientView : UserControl
    {
        public RegisterPatientView()
        {
            InitializeComponent();
            DataContext = new RegisterPatientViewModel();
        }
    }
}
