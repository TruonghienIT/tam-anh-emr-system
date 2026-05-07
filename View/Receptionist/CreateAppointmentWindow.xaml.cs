using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.ViewModel.Receptionist;

namespace TamAnh_EMR_System.View.Receptionist
{
    /// <summary>
    /// Code-behind for CreateAppointmentWindow.
    /// 
    /// Minimal code-behind responsibilities:
    /// - Create and wire the ViewModel
    /// - Provide CloseAction callback so ViewModel can close the window
    /// - Handle title bar drag
    /// - Handle close button click
    /// 
    /// All business logic stays in CreateAppointmentViewModel.
    /// </summary>
    public partial class CreateAppointmentWindow : Window
    {
        public CreateAppointmentWindow()
        {
            InitializeComponent();

            // Create ViewModel with close callback
            var vm = new CreateAppointmentViewModel();

            // Allow ViewModel to close this window and set DialogResult
            vm.CloseAction = (result) =>
            {
                DialogResult = result;
                Close();
            };

            // Set DataContext on the embedded UserControl (not the Window)
            // so all bindings in CreateAppointmentView.xaml resolve correctly
            AppointmentContent.DataContext = vm;

            // Load initial data from database
            vm.InitializeAsync();
        }

        /// <summary>
        /// Allows dragging the window by the custom title bar.
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
                DragMove();
        }

        /// <summary>
        /// Close button click — cancel without saving.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
