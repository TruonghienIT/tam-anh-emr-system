using System;
using System.Windows;

namespace TamAnh_EMR_System.View.Doctor
{
    public partial class RescheduleWindow : Window
    {
        public DateTime SelectedDate { get; private set; }
        public string SelectedTime { get; private set; }

        public RescheduleWindow(DateTime currentDate, string currentTime)
        {
            InitializeComponent();
            Dp.SelectedDate = currentDate;
            TbTime.Text = currentTime;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!Dp.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng chọn ngày", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedDate = Dp.SelectedDate.Value;
            SelectedTime = TbTime.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
