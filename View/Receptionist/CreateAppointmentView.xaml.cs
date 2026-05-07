using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using TamAnh_EMR_System.ViewModel.Receptionist;

namespace TamAnh_EMR_System.View.Receptionist
{
    public partial class CreateAppointmentView : UserControl
    {
        public CreateAppointmentView()
        {
            InitializeComponent();
        }

        private void RootGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CreateAppointmentViewModel vm)
            {
                if (!vm.IsPatientPopupOpen && !vm.ShowSearchResults)
                    return;

                DependencyObject source = e.OriginalSource as DependencyObject;

                while (source != null)
                {
                    if (source is FrameworkElement fe)
                    {
                        if (fe.Name == "SearchTextBox" ||
                            fe.Name == "SearchPopupBorder" ||
                            fe.Name == "AllPatientsPopupBorder")
                        {
                            return;
                        }
                    }

                    if (source is Visual || source is Visual3D)
                    {
                        source = VisualTreeHelper.GetParent(source);
                    }
                    else
                    {
                        break;
                    }
                }

                vm.IsPatientPopupOpen = false;
                vm.ShowSearchResults = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}