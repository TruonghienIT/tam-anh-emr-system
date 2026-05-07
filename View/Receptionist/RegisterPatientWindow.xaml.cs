using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.ViewModel;

namespace TamAnh_EMR_System.View.Receptionist
{
    public partial class RegisterPatientWindow : Window
    {
        public RegisterPatientWindow()
        {
            InitializeComponent();
            DataContext = new RegisterPatientViewModel();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
