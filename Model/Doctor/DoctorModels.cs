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
        public string Label { get; set; }
        public string Tooltip { get; set; }
        public double BarHeight { get; set; }

        public DoctorDashboardData()
        {
        }

        public DoctorDashboardData(string label, double value, double maxValue = 100)
        {
            Label = label;
            Value = value;
            Tooltip = $"{label}: {value:F0} bệnh nhân";
            // Normalize height để fit trong chart (max 180px)
            BarHeight = maxValue > 0 ? (value / maxValue) * 180 : 0;
        }
    }
}
