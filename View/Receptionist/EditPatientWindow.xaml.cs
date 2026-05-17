using System.Windows;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.ViewModel.Receptionist;

namespace TamAnh_EMR_System.View.Receptionist
{
    public partial class EditPatientWindow : Window
    {
        public EditPatientWindow(Patients patient)
        {
            InitializeComponent();

            DataContext =
                new EditPatientViewModel(patient);
        }
    }
}