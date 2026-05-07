using System;
using System.ComponentModel;

namespace TamAnh_EMR_System.Model.Receptionist
{
    /// <summary>
    /// Model for the "Tạo lịch hẹn mới" form.
    /// Holds all data entered by the receptionist when scheduling an appointment.
    /// Implements INotifyPropertyChanged for two-way binding on form inputs.
    /// FUTURE: Map to API request body for backend integration.
    /// </summary>
    public class AppointmentForm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ======================== PATIENT INFO ========================

        private string _patientName;
        public string PatientName
        {
            get => _patientName;
            set { _patientName = value; OnPropertyChanged(nameof(PatientName)); }
        }

        private string _patientId;
        public string PatientId
        {
            get => _patientId;
            set { _patientId = value; OnPropertyChanged(nameof(PatientId)); }
        }

        private string _patientInitials;
        public string PatientInitials
        {
            get => _patientInitials;
            set { _patientInitials = value; OnPropertyChanged(nameof(PatientInitials)); }
        }

        private string _patientGender;
        public string PatientGender
        {
            get => _patientGender;
            set { _patientGender = value; OnPropertyChanged(nameof(PatientGender)); }
        }

        private int _patientAge;
        public int PatientAge
        {
            get => _patientAge;
            set { _patientAge = value; OnPropertyChanged(nameof(PatientAge)); }
        }

        // ======================== DEPARTMENT & DOCTOR ========================

        private string _department;
        public string Department
        {
            get => _department;
            set { _department = value; OnPropertyChanged(nameof(Department)); }
        }

        private string _doctor;
        public string Doctor
        {
            get => _doctor;
            set { _doctor = value; OnPropertyChanged(nameof(Doctor)); }
        }

        // ======================== APPOINTMENT DETAILS ========================

        private string _appointmentType = "Khám tổng quát";
        public string AppointmentType
        {
            get => _appointmentType;
            set { _appointmentType = value; OnPropertyChanged(nameof(AppointmentType)); }
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(nameof(SelectedDate)); }
        }

        private string _selectedTimeSlot;
        public string SelectedTimeSlot
        {
            get => _selectedTimeSlot;
            set { _selectedTimeSlot = value; OnPropertyChanged(nameof(SelectedTimeSlot)); }
        }

        private string _note;
        public string Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(nameof(Note)); }
        }

        private string _location = "Tầng 2, Phòng 204";
        public string Location
        {
            get => _location;
            set { _location = value; OnPropertyChanged(nameof(Location)); }
        }

        /// <summary>Summary display for patient info line</summary>
        public string PatientInfoLine => $"ID: {PatientId} • {PatientGender} • {PatientAge} tuổi";
    }
}
