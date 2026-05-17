using System.Windows.Controls;

namespace TamAnh_EMR_System.View.Doctor
{
    /// <summary>
    /// Interaction logic for MedicalRecordsView.xaml
    /// </summary>
    public partial class MedicalRecordsView : UserControl
    {
        public MedicalRecordsView()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.Doctor.MedicalRecordsViewModel();
        }
    }
}
