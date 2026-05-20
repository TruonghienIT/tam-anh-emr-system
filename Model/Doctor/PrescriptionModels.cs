using System;
using System.ComponentModel;

namespace TamAnh_EMR_System.Model.Doctor
{
    /// <summary>Prescription header shown in the left list table.</summary>
    public class Prescription : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnProp(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public string PatientName { get; set; }
        public string PatientId { get; set; }
        public DateTime Date { get; set; }
        public string DateDisplay => Date.ToString("dd/MM/yyyy");
        public string TimeDisplay => Date.ToString("hh:mm tt");
        public string DoctorName { get; set; }
        public string Status { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnProp(nameof(IsSelected)); }
        }
        // Thêm dòng này vào class Prescription hiện tại của bạn:
        public string RecordId { get; set; }
    }

    /// <summary>Medicine line item within a prescription.</summary>
    public class MedicineItem
    {
        public string MedicineId { get; set; }
        public string Name { get; set; }
        public string Dosage { get; set; }
        public string Instruction { get; set; }
        public int Quantity { get; set; }
        public string QuantityDisplay => $"{Quantity} Viên";
    }
}
