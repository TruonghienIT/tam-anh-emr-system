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

        // ================= THUỘC TÍNH TÌM KIẾM & LỌC =================
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                // Tự động lọc Local khi gõ text (Nếu bạn muốn bấm nút mới tìm thì bỏ dòng dưới đi)
                _ = FilterRecordsAsync();
            }
        }

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged(nameof(FromDate));
            }
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged(nameof(ToDate));
            }
        }

        // ================= COMMANDS =================
        public ICommand RefreshCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ViewDetailCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public MedicalRecordsViewModel()
        {
            _repository = new DoctorPatientManagementRepository();

            // Lệnh Làm mới: Xóa ngày tháng, text và load lại DB
            RefreshCommand = new RelayCommand(async _ =>
            {
                SearchText = string.Empty;
                FromDate = null;
                ToDate = null;
                await LoadRecordsAsync();
            });

            // Lệnh Lọc: Chủ động bấm nút lọc
            FilterCommand = new RelayCommand(async _ => await FilterRecordsAsync());

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

        private async Task OpenDetailWindowAsync(MedicalRecords record, bool isEditMode)
        {
            if (record == null) return;

            var detailWindow = new TamAnh_EMR_System.View.Doctor.MedicalRecordDetailWindow(record, isEditMode);
            bool? result = detailWindow.ShowDialog();

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
                // Lấy toàn bộ dữ liệu (Bao gồm cả Join Bệnh nhân, Sinh tồn, Xét nghiệm như đã sửa)
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
                        MessageBox.Show("Không có dữ liệu hồ sơ bệnh án.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= HÀM LỌC CHÍNH (TEXT + NGÀY THÁNG) =================
        private async Task FilterRecordsAsync()
        {
            if (FromDate.HasValue && ToDate.HasValue && FromDate.Value > ToDate.Value)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MedicalRecordsList.Clear();

                    var filtered = _allRecords.AsEnumerable();

                    // 1. Lọc theo text tìm kiếm
                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        var search = SearchText.ToLower();
                        filtered = filtered.Where(r =>
                            (r.Patient?.Name?.ToLower().Contains(search) ?? false) ||
                            (r.Diagnosis?.ToLower().Contains(search) ?? false) ||
                            (r.IcdCode?.ToLower().Contains(search) ?? false));
                    }

                    // 2. Lọc theo Từ ngày (Lấy mốc 00:00:00)
                    if (FromDate.HasValue)
                    {
                        filtered = filtered.Where(r => r.CreatedAt.Date >= FromDate.Value.Date);
                    }

                    // 3. Lọc theo Đến ngày (Lấy mốc 23:59:59)
                    if (ToDate.HasValue)
                    {
                        filtered = filtered.Where(r => r.CreatedAt.Date <= ToDate.Value.Date);
                    }

                    // Cập nhật lại UI
                    foreach (var r in filtered.OrderByDescending(r => r.CreatedAt))
                    {
                        MedicalRecordsList.Add(r);
                    }
                });
            });
        }
    }
}