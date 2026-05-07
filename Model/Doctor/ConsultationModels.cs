using System.ComponentModel;
using System.Collections.ObjectModel;

namespace TamAnh_EMR_System.Model.Doctor
{
    /// <summary>Vital signs measured during consultation.</summary>
    public class VitalSigns : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnProp(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        private int _pulse;
        public int Pulse { get => _pulse; set { _pulse = value; OnProp(nameof(Pulse)); } }

        private string _bloodPressure;
        public string BloodPressure { get => _bloodPressure; set { _bloodPressure = value; OnProp(nameof(BloodPressure)); } }

        private double _temperature;
        public double Temperature { get => _temperature; set { _temperature = value; OnProp(nameof(Temperature)); OnProp(nameof(IsTemperatureHigh)); } }
        public bool IsTemperatureHigh => Temperature > 38.0;

        private int _spO2;
        public int SpO2 { get => _spO2; set { _spO2 = value; OnProp(nameof(SpO2)); } }
    }

    /// <summary>Checkbox symptom item.</summary>
    public class SymptomItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); }
        }
    }

    /// <summary>Queue patient in left panel.</summary>
    public class QueuePatientItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnProp(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public string Name { get; set; }
        public string Initials { get; set; }
        public string AvatarColor { get; set; }
        public string GenderAge { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnProp(nameof(IsSelected)); } }
    }

    /// <summary>Main consultation/EMR data model.</summary>
    public class ConsultationModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnProp(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public string PatientName { get; set; }
        public string PatientId { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string BirthDate { get; set; }
        public string BloodType { get; set; }
        public string Insurance { get; set; }

        public string InfoLine => $"ID: {PatientId}   📅 {BirthDate} ({Age}t)   ♂ {Gender}   ⊕ {BloodType}";

        private VitalSigns _vitalSigns;
        public VitalSigns VitalSigns { get => _vitalSigns; set { _vitalSigns = value; OnProp(nameof(VitalSigns)); } }

        public ObservableCollection<SymptomItem> Symptoms { get; set; } = new ObservableCollection<SymptomItem>();

        private string _diagnosis;
        public string Diagnosis { get => _diagnosis; set { _diagnosis = value; OnProp(nameof(Diagnosis)); } }

        private string _notes;
        public string Notes { get => _notes; set { _notes = value; OnProp(nameof(Notes)); } }
    }
}
