using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;
using TamAnh_EMR_System.Services.Pdf;


namespace TamAnh_EMR_System.ViewModel.Receptionist
{
    public class AppointmentManagementViewModel : ViewModelBase
    {
        private readonly AppointmentRepository _repo;

        public ObservableCollection<AppointmentDisplay>
            Appointments
        { get; set; }

        public ObservableCollection<AppointmentDisplay>
            FilteredAppointments
        { get; set; }

        public ObservableCollection<string>
            StatusOptions
        { get; set; }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;

                OnPropertyChanged(nameof(SearchText));

                FilterAppointments();
            }
        }

        private string _selectedStatus = "Tất cả";

        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;

                OnPropertyChanged(nameof(SelectedStatus));

                FilterAppointments();
            }
        }

        public ICommand RefreshCommand { get; }

        public ICommand DeleteCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public AppointmentManagementViewModel()
        {
            _repo = new AppointmentRepository();

            Appointments =
                new ObservableCollection<AppointmentDisplay>();

            FilteredAppointments =
                new ObservableCollection<AppointmentDisplay>();

            StatusOptions =
                new ObservableCollection<string>
                {
                    "Tất cả",
                    "Đang chờ",
                    "Đã xác nhận",
                    "Đang khám",
                    "Hoàn thành",
                    "Đã hủy"
                };

            RefreshCommand =
                new RelayCommand(async _ =>
                    await LoadAsync());

            DeleteCommand =
                new RelayCommand(async p =>
                {
                    if (p is AppointmentDisplay item)
                    {
                        await _repo.DeleteAsync(item.Id);

                        await LoadAsync();
                    }
                });
            ExportPdfCommand =
                new RelayCommand(p =>
                {
                    if (p is AppointmentDisplay item)
                    {
                        try
                        {
                            var pdfService =
                                new AppointmentPdfService();

                            pdfService.Export(item);

                            MessageBox.Show(
                                "Xuất PDF thành công!",
                                "Thông báo");
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show(
                                ex.Message,
                                "Lỗi export PDF");
                        }
                    }
                });

            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (Appointments.Any())
            {
                Appointments.Clear();
            }

            var data =
                await _repo.GetAllDisplayAsync();

            foreach (var item in data)
            {
                Appointments.Add(item);
            }

            FilterAppointments();
        }

        private void FilterAppointments()
        {
            if (Appointments == null)
                return;

            FilteredAppointments.Clear();

            var query = Appointments.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.ToLower();

                query = query.Where(x =>
                    (x.PatientName ?? "")
                        .ToLower()
                        .Contains(keyword)

                    ||

                    (x.DoctorName ?? "")
                        .ToLower()
                        .Contains(keyword)

                    ||

                    (x.Id ?? "")
                        .ToLower()
                        .Contains(keyword)

                    ||

                    (x.Department ?? "")
                        .ToLower()
                        .Contains(keyword)
                );
            }

            if (!string.IsNullOrWhiteSpace(SelectedStatus)
                &&
                SelectedStatus != "Tất cả")
            {
                query = query.Where(x =>
                    (x.Status ?? "") == SelectedStatus);
            }

            foreach (var item in query)
            {
                FilteredAppointments.Add(item);
            }
        }
    }
}