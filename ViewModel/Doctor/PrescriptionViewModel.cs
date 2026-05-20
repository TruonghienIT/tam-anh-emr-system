using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model.Doctor;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel.Doctor
{
    public class PrescriptionViewModel : ViewModelBase
    {
        private readonly PrescriptionRepository _repository;

        // Danh sách Đơn thuốc (Trái)
        public ObservableCollection<Prescription> Prescriptions { get; set; }

        // Danh sách Thuốc chi tiết (Phải)
        public ObservableCollection<MedicineItem> SelectedPrescriptionDetails { get; set; }

        // Biến lưu Đơn thuốc đang được chọn
        private Prescription _selectedPrescription;
        public Prescription SelectedPrescription
        {
            get => _selectedPrescription;
            set
            {
                _selectedPrescription = value;
                OnPropertyChanged(nameof(SelectedPrescription));

                // Khi click chọn toa thuốc khác, lập tức tải danh sách thuốc tương ứng
                if (_selectedPrescription != null)
                {
                    _ = LoadMedicineDetailsAsync(_selectedPrescription.RecordId);
                }
            }
        }

        public ICommand PrintCommand { get; }
        public ICommand AddPatientCommand { get; }

        public ICommand RemovePatientCommand { get; }

        public ICommand SelectPrescriptionCommand { get; }

        public PrescriptionViewModel()
        {
            _repository = new PrescriptionRepository();
            Prescriptions = new ObservableCollection<Prescription>();
            SelectedPrescriptionDetails = new ObservableCollection<MedicineItem>();

            PrintCommand = new RelayCommand(_ => MessageBox.Show("Đang kết nối máy in...", "In Đơn Thuốc"));
            AddPatientCommand = new RelayCommand(_ => MessageBox.Show("Mở form thêm hồ sơ mới", "Tra Cứu"));

            // THÊM DÒNG NÀY ĐỂ XỬ LÝ KHI CLICK CHỌN TOA THUỐC
            SelectPrescriptionCommand = new RelayCommand(p =>
            {
                if (p is Prescription selected)
                {
                    SelectedPrescription = selected;
                }
            });

            _ = LoadPrescriptionsAsync();
        }

        public async Task LoadPrescriptionsAsync()
        {
            var data = await _repository.GetAllPrescriptionsAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Prescriptions.Clear();
                foreach (var item in data)
                {
                    Prescriptions.Add(item);
                }
            });
        }

        private async Task LoadMedicineDetailsAsync(string recordId)
        {
            var details = await _repository.GetMedicineDetailsAsync(recordId);
            Application.Current.Dispatcher.Invoke(() =>
            {
                SelectedPrescriptionDetails.Clear();
                foreach (var item in details)
                {
                    SelectedPrescriptionDetails.Add(item);
                }
            });
        }
    }
}