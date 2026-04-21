using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel
{
    public class DoctorPanelViewModel : ViewModelBase
    {
        private DoctorPanelRepository repository;

        public ObservableCollection<Doctors> Doctors { get; set; }

        private Doctors _selectedDoctor;
        public Doctors SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                _selectedDoctor = value;
                OnPropertyChanged(nameof(SelectedDoctor));
            }
        }

        // ================= POPUP STATE =================
        public bool IsPopupOpen { get; set; }
        public bool IsEditMode { get; set; }

        public Doctors CurrentDoctor { get; set; } = new Doctors();

        // ================= COMMAND =================
        public ICommand LoadDoctorCommand { get; }
        public ICommand OpenAddCommand { get; }
        public ICommand OpenEditCommand { get; }
        public ICommand SaveDoctorCommand { get; }
        public ICommand DeleteDoctorCommand { get; }
        public ICommand ClosePopupCommand { get; }

        public DoctorPanelViewModel()
        {
            repository = new DoctorPanelRepository();

            LoadDoctorCommand = new ViewModelCommand(_ => LoadDoctors());

            OpenAddCommand = new ViewModelCommand(_ =>
            {
                CurrentDoctor = new Doctors();
                IsEditMode = false;
                IsPopupOpen = true;
                OnPropertyChanged(nameof(CurrentDoctor));
                OnPropertyChanged(nameof(IsPopupOpen));
            });

            OpenEditCommand = new ViewModelCommand(obj =>
            {
                var doctor = obj as Doctors;
                if (doctor == null) return;

                CurrentDoctor = new Doctors
                {
                    Id = doctor.Id,
                    UserId = doctor.UserId,
                    FullName = doctor.FullName,
                    Email = doctor.Email,
                    Phone = doctor.Phone,
                    Specialization = doctor.Specialization
                };

                IsEditMode = true;
                IsPopupOpen = true;

                OnPropertyChanged(nameof(CurrentDoctor));
                OnPropertyChanged(nameof(IsPopupOpen));
            });

            SaveDoctorCommand = new ViewModelCommand(_ =>
            {
                if (IsEditMode)
                    repository.UpdateDoctor(CurrentDoctor);
                else
                    repository.AddDoctor(CurrentDoctor);

                LoadDoctors();

                IsPopupOpen = false;
                OnPropertyChanged(nameof(IsPopupOpen));
            });

            DeleteDoctorCommand = new ViewModelCommand(obj =>
            {
                var doctor = obj as Doctors;
                if (doctor == null) return;

                repository.DeleteDoctor(doctor.UserId);
                LoadDoctors();
            });

            ClosePopupCommand = new ViewModelCommand(_ =>
            {
                IsPopupOpen = false;
                OnPropertyChanged(nameof(IsPopupOpen));
            });

            LoadDoctors();
        }

        // ================= LOAD =================
        private void LoadDoctors()
        {
            Doctors = repository.GetAllDoctors();
            OnPropertyChanged(nameof(Doctors));
        }

        // ================= SEARCH =================
        private string _searchText;
        private CancellationTokenSource _cts;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                SearchAsync();
            }
        }

        private async void SearchAsync()
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();

                await Task.Delay(300, _cts.Token);

                if (string.IsNullOrWhiteSpace(_searchText))
                {
                    LoadDoctors();
                }
                else
                {
                    Doctors = repository.SearchDoctors(_searchText);
                    OnPropertyChanged(nameof(Doctors));
                }
            }
            catch (TaskCanceledException) { }
        }
    }
}