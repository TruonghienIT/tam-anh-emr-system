using System;
using System.Collections.ObjectModel;
using System.Windows;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Model.Doctor;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.View.Doctor
{
    public partial class AddPrescriptionDialog : Window
    {
        private DoctorPatientManagementRepository _repo;
        public ObservableCollection<MedicineItem> PrescriptionItems { get; private set; }

        public AddPrescriptionDialog(ObservableCollection<MedicineItem> currentItems = null)
        {
            InitializeComponent();
            _repo = new DoctorPatientManagementRepository();

            PrescriptionItems = currentItems ?? new ObservableCollection<MedicineItem>();
            DgPrescription.ItemsSource = PrescriptionItems;

            LoadMedicinesList();
        }

        private async void LoadMedicinesList()
        {
            try
            {
                var medicineList = await _repo.GetMedicinesAsync();
                CmbMedicines.ItemsSource = medicineList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh mục thuốc: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddMedicine_Click(object sender, RoutedEventArgs e)
        {
            if (CmbMedicines.SelectedItem is Medicines selectedMed)
            {
                if (!int.TryParse(TxtQuantity.Text, out int qty) || qty <= 0)
                {
                    MessageBox.Show("Vui lòng nhập số lượng thuốc hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Gộp Tần suất và Ghi chú phân tách bằng dấu gạch đứng để dễ bóc tách khi lưu DB
                string frequency = string.IsNullOrWhiteSpace(TxtFrequency.Text) ? "1 lần/ngày" : TxtFrequency.Text.Trim();
                string notes = string.IsNullOrWhiteSpace(TxtNotes.Text) ? "Uống sau ăn" : TxtNotes.Text.Trim();

                PrescriptionItems.Add(new MedicineItem
                {
                    MedicineId = selectedMed.Id,
                    Name = selectedMed.Name,
                    Quantity = qty,
                    Dosage = string.IsNullOrWhiteSpace(TxtDosage.Text) ? "500mg" : TxtDosage.Text.Trim(),
                    Instruction = $"{frequency} | {notes}"
                });

                CmbMedicines.SelectedIndex = -1;
                TxtQuantity.Clear();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn loại thuốc trước khi thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRemoveMedicine_Click(object sender, RoutedEventArgs e)
        {
            if (DgPrescription.SelectedItem is MedicineItem target)
            {
                PrescriptionItems.Remove(target);
            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}