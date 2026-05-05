using System;
using System.ComponentModel;
using System.Windows.Media;

namespace TamAnh_EMR_System.Model.Receptionist
{
    /// <summary>
    /// Model for a patient row in the "Bệnh nhân gần đây" table.
    /// Implements INotifyPropertyChanged so IsCheckedIn can update the UI in real-time.
    /// </summary>
    public class PatientSearchResult : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public string Name { get; set; }
        public string Initials { get; set; }
        public string AvatarColor { get; set; }
        public string PatientCode { get; set; }
        public string DateOfBirthText { get; set; }
        public string LastVisitText { get; set; }
        public string LastVisitColor { get; set; }
        public string LastVisitBg { get; set; }

        private bool _isCheckedIn;
        public bool IsCheckedIn
        {
            get => _isCheckedIn;
            set { _isCheckedIn = value; OnPropertyChanged(nameof(IsCheckedIn)); }
        }
    }
}
