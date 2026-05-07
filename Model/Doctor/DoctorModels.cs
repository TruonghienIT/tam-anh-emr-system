using System.ComponentModel;

namespace TamAnh_EMR_System.Model.Doctor
{
    /// <summary>Patient in the waiting queue panel on the right side.</summary>
    public class QueuePatient : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnProp(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public string Name { get; set; }
        public string Initials { get; set; }
        public string AvatarColor { get; set; }
        public string WaitingTime { get; set; }

        private bool _isUrgent;
        public bool IsUrgent
        {
            get => _isUrgent;
            set { _isUrgent = value; OnProp(nameof(IsUrgent)); }
        }
    }

    /// <summary>Appointment row in "Lịch hẹn hôm nay" table.</summary>
    public class AppointmentItem
    {
        public string Time { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Status { get; set; }
    }

    /// <summary>Holds chart data points for statistics cards.</summary>
    public class DoctorDashboardData
    {
        public double Value { get; set; }
        public double BarHeight => Value * 1.2;
    }
}
