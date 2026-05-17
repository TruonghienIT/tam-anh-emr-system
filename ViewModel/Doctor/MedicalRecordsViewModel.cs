using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel.Doctor
{
    public class MedicalRecordsViewModel : ViewModelBase
    {
        private readonly DoctorPatientManagementRepository _repository;

        public ObservableCollection<MedicalRecords> MedicalRecordsList { get; set; } = new ObservableCollection<MedicalRecords>();

        private ObservableCollection<MedicalRecords> _allRecords = new ObservableCollection<MedicalRecords>();

        private MedicalRecords _selectedRecord;
        public MedicalRecords SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                _selectedRecord = value;
                OnPropertyChanged(nameof(SelectedRecord));
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                _ = FilterRecordsAsync();
            }
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                _ = FilterRecordsAsync();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ViewDetailCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public MedicalRecordsViewModel()
        {
            _repository = new DoctorPatientManagementRepository();
            RefreshCommand = new RelayCommand(async _ => await LoadRecordsAsync());

            ViewDetailCommand = new RelayCommand(async p => await OpenDetailWindowAsync(p as MedicalRecords, false));
            EditCommand = new RelayCommand(async p => await OpenDetailWindowAsync(p as MedicalRecords, true));

            // --- LỆNH XÓA ---
            DeleteCommand = new RelayCommand(async p =>
            {
                if (p is MedicalRecords record)
                {
                    var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa hồ sơ khám của bệnh nhân {record.Patient?.Name}?",
                                                  "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (confirm == MessageBoxResult.Yes)
                    {
                        try
                        {
                            bool success = await _repository.DeleteMedicalRecordAsync(record.Id);
                            if (success)
                            {
                                MedicalRecordsList.Remove(record);
                                _allRecords.Remove(record);
                                MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Không thể xóa bệnh án (Có thể do ràng buộc khóa ngoại toa thuốc/xét nghiệm).\nLỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            });

            _ = LoadRecordsAsync();
        }

        // Hàm mở Form Chi tiết / Sửa
        // Thêm tham số bool isEditMode
        private async Task OpenDetailWindowAsync(MedicalRecords record, bool isEditMode)
        {
            if (record == null) return;

            // Truyền cờ isEditMode vào Window
            var detailWindow = new TamAnh_EMR_System.View.Doctor.MedicalRecordDetailWindow(record, isEditMode);
            bool? result = detailWindow.ShowDialog();

            // Nếu người dùng bấm "Lưu Thay Đổi" (Và chỉ khi đang ở chế độ sửa)
            if (result == true && detailWindow.IsSaved)
            {
                try
                {
                    bool isUpdated = await _repository.UpdateMedicalRecordAsync(detailWindow.RecordData);
                    if (isUpdated)
                    {
                        MessageBox.Show("Cập nhật bệnh án thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadRecordsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Có lỗi xảy ra, không thể cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi hệ thống: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task LoadRecordsAsync()
        {
            try
            {
                var records = await _repository.GetAllMedicalRecordsAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _allRecords.Clear();
                    MedicalRecordsList.Clear();

                    foreach (var r in records)
                    {
                        _allRecords.Add(r);
                        MedicalRecordsList.Add(r);
                    }

                    if (MedicalRecordsList.Count == 0)
                    {
                        MessageBox.Show(
                            "Không có dữ liệu hồ sơ bệnh án.",
                            "Thông báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi tải dữ liệu:\n{ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task FilterRecordsAsync()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MedicalRecordsList.Clear();

                    var filtered = _allRecords.AsEnumerable();

                    // Filter by search text
                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        var search = SearchText.ToLower();
                        filtered = filtered.Where(r =>
                            (r.Patient?.Name?.ToLower().Contains(search) ?? false) ||
                            (r.Doctor?.FullName?.ToLower().Contains(search) ?? false) ||
                            (r.Diagnosis?.ToLower().Contains(search) ?? false) ||
                            (r.IcdCode?.ToLower().Contains(search) ?? false));
                    }

                    // Add to list
                    foreach (var r in filtered.OrderByDescending(r => r.CreatedAt))
                    {
                        MedicalRecordsList.Add(r);
                    }
                });
            });
        }
    }
}
