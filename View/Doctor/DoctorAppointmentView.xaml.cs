using System.Windows.Controls;
using TamAnh_EMR_System.ViewModel.Doctor;

namespace TamAnh_EMR_System.View.Doctor
{
    public partial class DoctorAppointmentView : UserControl
    {
        public DoctorAppointmentView()
        {
            InitializeComponent();
            this.DataContext = new DoctorAppointmentViewModel();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
