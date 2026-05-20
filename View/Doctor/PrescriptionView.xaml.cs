using System.Windows;
using System.Windows.Controls;
using TamAnh_EMR_System.ViewModel.Doctor;

namespace TamAnh_EMR_System.View.Doctor
{
    public partial class PrescriptionView : UserControl
    {
        private bool _isLoading = false;

        public PrescriptionView()
        {
            InitializeComponent();
            DataContext = new PrescriptionViewModel();
        }

        private async void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible || _isLoading)
                return;

            if (DataContext is PrescriptionViewModel vm)
            {
                _isLoading = true;
                await vm.LoadPrescriptionsAsync();
                _isLoading = false;
            }
        }
    }
}