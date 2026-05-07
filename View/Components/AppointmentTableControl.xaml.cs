using System.Windows.Controls;

namespace TamAnh_EMR_System.View.Components
{
    /// <summary>
    /// Code-behind for AppointmentTableControl.
    /// DataContext inherited from parent. No business logic.
    /// </summary>
    public partial class AppointmentTableControl : UserControl
    {
        public AppointmentTableControl()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
