using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model.Doctor;

namespace TamAnh_EMR_System.ViewModel.Doctor
{
    /// <summary>
    /// ViewModel for Doctor Prescription screen ("Đơn thuốc").
    /// Left panel: search + prescription list. Right panel: detail + medicines.
    /// FUTURE: Replace sample data with API calls.
    /// </summary>
    public class PrescriptionViewModel : ViewModelBase
    {
        // ===== SEARCH =====
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        // ===== PRESCRIPTION LIST =====
        public ObservableCollection<Prescription> Prescriptions { get; set; }

        private Prescription _selectedPrescription;
        public Prescription SelectedPrescription
        {
            get => _selectedPrescription;
            set
            {
                if (_selectedPrescription != null) _selectedPrescription.IsSelected = false;
                _selectedPrescription = value;
                if (_selectedPrescription != null) _selectedPrescription.IsSelected = true;
                OnPropertyChanged(nameof(SelectedPrescription));
                OnPropertyChanged(nameof(HasSelection));
                LoadMedicinesForPrescription();
            }
        }

        public bool HasSelection => SelectedPrescription != null;

        // ===== MEDICINE DETAIL =====
        public ObservableCollection<MedicineItem> Medicines { get; set; }

        // ===== COMMANDS =====
        public ICommand SearchCommand { get; }
        public ICommand AddPatientCommand { get; }
        public ICommand SelectPrescriptionCommand { get; }
        public ICommand PrintCommand { get; }

        public PrescriptionViewModel()
        {
            Prescriptions = new ObservableCollection<Prescription>();
            Medicines = new ObservableCollection<MedicineItem>();

            SearchCommand = new RelayCommand(_ => MessageBox.Show($"Tìm kiếm: {SearchText}", "Tìm kiếm"));
            AddPatientCommand = new RelayCommand(_ => MessageBox.Show("Thêm bệnh nhân mới", "Thêm"));
            SelectPrescriptionCommand = new RelayCommand(p => { if (p is Prescription rx) SelectedPrescription = rx; });
            PrintCommand = new RelayCommand(_ => MessageBox.Show($"In đơn thuốc cho {SelectedPrescription?.PatientName}", "In đơn thuốc"));

            LoadSampleData();
        }

        private void LoadMedicinesForPrescription()
        {
            Medicines.Clear();
            if (SelectedPrescription == null) return;

            if (SelectedPrescription.Status == "Mới kê")
            {
                Medicines.Add(new MedicineItem { Name = "Amoxicillin 500mg", Dosage = "Uống 2 lần/ngày, mỗi lần 1 viên.", Instruction = "Sau bữa ăn.", Quantity = 20 });
                Medicines.Add(new MedicineItem { Name = "Paracetamol 500mg", Dosage = "Uống khi đau hoặc sốt > 38.5 độ.", Instruction = "Cách nhau ít nhất 4 tiếng.", Quantity = 10 });
            }
            else
            {
                Medicines.Add(new MedicineItem { Name = "Omeprazole 20mg", Dosage = "Uống 1 lần/ngày trước ăn sáng.", Instruction = "Không nhai viên thuốc.", Quantity = 14 });
            }
        }

        private void LoadSampleData()
        {
            var rx1 = new Prescription
            {
                PatientName = "Nguyễn Văn A", PatientId = "BN-2023-0891",
                Date = new DateTime(2023, 10, 12, 9, 30, 0),
                DoctorName = "BS. Trần Thanh Bình", Status = "Mới kê"
            };
            var rx2 = new Prescription
            {
                PatientName = "Trần Thị B", PatientId = "BN-2023-0742",
                Date = new DateTime(2023, 10, 12, 8, 15, 0),
                DoctorName = "BS. Lê Hoàng", Status = "Đã nhận"
            };

            Prescriptions.Add(rx1);
            Prescriptions.Add(rx2);
            SelectedPrescription = rx1;
        }
    }
}
