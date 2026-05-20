using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TamAnh_EMR_System.Model.Doctor; 
using TamAnh_EMR_System.ViewModel.Doctor;

namespace TamAnh_EMR_System.View.Doctor
{
    public partial class DoctorPatientManagementView : UserControl
    {
        public DoctorPatientManagementView()
        {
            InitializeComponent();
            this.DataContext = new DoctorPatientManagementViewModel();
        }
        private void BtnOpenPrescription_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is DoctorPatientManagementViewModel viewModel)
            {
                // Truyền danh sách thuốc từ ViewModel vào Dialog
                var dialog = new AddPrescriptionDialog(viewModel.CurrentPrescription);
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();
            }
        }
    }
}