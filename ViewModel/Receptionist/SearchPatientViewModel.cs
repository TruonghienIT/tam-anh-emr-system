using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;
using TamAnh_EMR_System.View.Receptionist;

namespace TamAnh_EMR_System.ViewModel.Receptionist
{
    public class SearchPatientViewModel : ViewModelBase
    {
        private readonly IPatientRepository _repo;
        private readonly AppointmentRepository _appointmentRepo;
        public ObservableCollection<ToastMessage> Toasts { get; set; }
        private CancellationTokenSource _cts;

        public ObservableCollection<Patients> Patients { get; set; }
        public ObservableCollection<string> GenderOptions { get; set; }
        public ObservableCollection<string> BloodOptions { get; set; }

        // =====================================================
        // PAGINATION PROPERTIES (BƯỚC 1)
        // =====================================================

        public ObservableCollection<Patients> PagedPatients { get; set; }
        public ObservableCollection<int> PageNumbers { get; set; }

        private List<Patients> _filteredPatients;
        private const int PageSize = 10;
        private int _currentPage = 1;

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                OnPropertyChanged(nameof(PaginationText));
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
            }
        }

        public string PaginationText
        {
            get
            {
                if (_filteredPatients == null || _filteredPatients.Count == 0)
                {
                    return "Không có dữ liệu";
                }

                int start = ((CurrentPage - 1) * PageSize) + 1;
                int end = Math.Min(CurrentPage * PageSize, _filteredPatients.Count);

                return $"Hiển thị {start} - {end} / {_filteredPatients.Count}";
            }
        }

        // =====================================================
        // DASHBOARD STATS
        // =====================================================

        private int _checkedInToday;
        public int CheckedInToday
        {
            get => _checkedInToday;
            set
            {
                _checkedInToday = value;
                OnPropertyChanged(nameof(CheckedInToday));
            }
        }

        private int _waitingCount;
        public int WaitingCount
        {
            get => _waitingCount;
            set
            {
                _waitingCount = value;
                OnPropertyChanged(nameof(WaitingCount));
            }
        }

        // =====================================================
        // SEARCH
        // =====================================================

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                _ = SearchAsync();
            }
        }

        // =====================================================
        // FILTERS
        // =====================================================

        private string _selectedGender = "Tất cả";
        public string SelectedGender
        {
            get => _selectedGender;
            set
            {
                _selectedGender = value;
                OnPropertyChanged(nameof(SelectedGender));
                _ = LoadPatientsAsync();
            }
        }

        private string _selectedBlood = "Tất cả";
        public string SelectedBlood
        {
            get => _selectedBlood;
            set
            {
                _selectedBlood = value;
                OnPropertyChanged(nameof(SelectedBlood));
                _ = LoadPatientsAsync();
            }
        }

        // =====================================================
        // LOADING
        // =====================================================

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        // =====================================================
        // COMMANDS
        // =====================================================

        public ICommand DeletePatientCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand EditPatientCommand { get; }
        public ICommand NavigatePageCommand { get; } // Thêm từ BƯỚC 2

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public SearchPatientViewModel()
        {
            Toasts = new ObservableCollection<ToastMessage>();
            _repo = new PatientRepository();
            _appointmentRepo = new AppointmentRepository();
            Patients = new ObservableCollection<Patients>();

            // Khởi tạo phân trang (BƯỚC 2)
            PagedPatients = new ObservableCollection<Patients>();
            PageNumbers = new ObservableCollection<int>();

            GenderOptions = new ObservableCollection<string>
            {
                "Tất cả",
                "Nam",
                "Nữ"
            };

            BloodOptions = new ObservableCollection<string>
            {
                "Tất cả",
                "A",
                "B",
                "AB",
                "O"
            };

            DeletePatientCommand = new RelayCommand(async p => await DeletePatient(p));
            RefreshCommand = new RelayCommand(async _ => await LoadPatientsAsync());
            EditPatientCommand = new RelayCommand(EditPatient);
            NavigatePageCommand = new RelayCommand(ExecuteNavigatePage); // Thêm từ BƯỚC 2

            _ = LoadPatientsAsync();
            _ = InitializeAsync();
        }

        // =====================================================
        // PAGINATION METHODS (BƯỚC 3)
        // =====================================================

        private void ExecuteNavigatePage(object p)
        {
            if (p == null) return;
            if (!int.TryParse(p.ToString(), out int page)) return;
            if (page < 1 || page > TotalPages) return;

            CurrentPage = page;
            UpdatePagedPatients();
        }

        private void UpdatePagedPatients()
        {
            PagedPatients.Clear();

            if (_filteredPatients == null) return;

            var items = _filteredPatients
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize);

            foreach (var item in items)
            {
                PagedPatients.Add(item);
            }

            OnPropertyChanged(nameof(PaginationText));
        }

        private void GeneratePagination()
        {
            PageNumbers.Clear();

            for (int i = 1; i <= TotalPages; i++)
            {
                PageNumbers.Add(i);
            }
        }

        // =====================================================
        // LOAD
        // =====================================================

        private async Task LoadPatientsAsync()
        {
            try
            {
                IsLoading = true;

                var data = await _repo.SearchWithFilterAsync(
                    SearchText,
                    SelectedGender,
                    SelectedBlood
                );

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Tích hợp BƯỚC 4
                    _filteredPatients = data.ToList();

                    TotalPages = Math.Max(1, (int)Math.Ceiling((double)_filteredPatients.Count / PageSize));

                    if (CurrentPage > TotalPages)
                    {
                        CurrentPage = TotalPages;
                    }
                    else if (CurrentPage < 1)
                    {
                        CurrentPage = 1;
                    }

                    GeneratePagination();
                    UpdatePagedPatients();
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDashboardStatsAsync()
        {
            var stats = await _appointmentRepo.GetTodayStatisticsAsync();

            CheckedInToday = stats.ContainsKey("total") ? stats["total"] : 0;
            WaitingCount = stats.ContainsKey("waiting") ? stats["waiting"] : 0;
        }

        private async Task InitializeAsync()
        {
            await LoadPatientsAsync();
            await LoadDashboardStatsAsync();
        }

        // =====================================================
        // SEARCH DEBOUNCE
        // =====================================================

        private async Task SearchAsync()
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                await Task.Delay(300, _cts.Token);
                await LoadPatientsAsync();
            }
            catch
            {
            }
        }

        // =====================================================
        // DELETE & EDIT
        // =====================================================

        private async Task DeletePatient(object p)
        {
            if (p is not Patients patient) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa bệnh nhân:\n{patient.Name} ?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirm != MessageBoxResult.Yes) return;

            await _repo.DeleteAsync(patient.Id);
            await LoadPatientsAsync();

            ShowToast("Xóa thành công", $"Đã xóa bệnh nhân {patient.Name}");
        }

        private void EditPatient(object p)
        {
            if (p is not Patients patient) return;

            var window = new EditPatientWindow(patient);
            window.ShowDialog();

            _ = LoadPatientsAsync();
        }

        private async void ShowToast(string title, string message, string type = "success")
        {
            var toast = new ToastMessage
            {
                Title = title,
                Message = message,
                Background = type == "error"
                    ? System.Windows.Media.Brushes.IndianRed
                    : System.Windows.Media.Brushes.SeaGreen,

                BorderBrush = type == "error"
                    ? System.Windows.Media.Brushes.DarkRed
                    : System.Windows.Media.Brushes.DarkGreen
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                Toasts.Add(toast);
            });

            await Task.Delay(3000);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Toasts.Remove(toast);
            });
        }
    }
}