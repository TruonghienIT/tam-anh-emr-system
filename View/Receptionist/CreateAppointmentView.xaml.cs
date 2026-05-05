using System.Windows.Controls;
using TamAnh_EMR_System.ViewModel.Receptionist;

namespace TamAnh_EMR_System.View.Receptionist
{
    /// <summary>
    /// Code-behind for CreateAppointmentView.
    /// Only sets DataContext — all logic in CreateAppointmentViewModel.
    /// </summary>
    public partial class CreateAppointmentView : UserControl
    {
        public CreateAppointmentView()
        {
            InitializeComponent();
            DataContext = new CreateAppointmentViewModel();
        }
    }
}
