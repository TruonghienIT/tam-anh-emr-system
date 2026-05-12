using System.Windows.Controls;
using TamAnh_EMR_System.ViewModel.Receptionist;

namespace TamAnh_EMR_System.View.Receptionist
{
    /// <summary>
    /// Interaction logic for CreateAppointmentWindow.xaml
    /// </summary>
    public partial class CreateAppointmentWindow : UserControl
    {
        public CreateAppointmentWindow()
        {
            InitializeComponent();

            // Create ViewModel
            var vm = new CreateAppointmentViewModel();

            // Set DataContext for entire UserControl
            DataContext = vm;

            // Load initial data
            vm.InitializeAsync();
        }
    }
}