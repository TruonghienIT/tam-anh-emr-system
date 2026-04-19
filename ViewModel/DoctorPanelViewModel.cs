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

        // ================= COMMAND =================
        public ICommand LoadDoctorCommand { get; }
        public ICommand AddDoctorCommand { get; }
        public ICommand UpdateDoctorCommand { get; }
        public ICommand DeleteDoctorCommand { get; }

        public DoctorPanelViewModel()
        {
            repository = new DoctorPanelRepository();

            LoadDoctorCommand = new ViewModelCommand(ExecuteLoad);
            AddDoctorCommand = new ViewModelCommand(ExecuteAdd);
            UpdateDoctorCommand = new ViewModelCommand(ExecuteUpdate, CanExecute);
            DeleteDoctorCommand = new ViewModelCommand(ExecuteDelete, CanExecute);

            LoadDoctors();
        }

        // ================= LOAD =================
        private void ExecuteLoad(object obj)
        {
            LoadDoctors();
        }

        private void LoadDoctors()
        {
            Doctors = repository.GetAllDoctors();
            OnPropertyChanged(nameof(Doctors));
        }

        // ================= ADD =================
        private void ExecuteAdd(object obj)
        {
            var doctor = new Doctors
            {
                FullName = "New Doctor",
                Email = "doctor@gmail.com",
                Phone = "0000000000",
                Specialization = "General"
            };

            repository.AddDoctor(doctor);
            LoadDoctors();
        }

        // ================= UPDATE =================
        private void ExecuteUpdate(object obj)
        {
            if (SelectedDoctor == null) return;

            repository.UpdateDoctor(SelectedDoctor);
            LoadDoctors();
        }

        // ================= DELETE =================
        private void ExecuteDelete(object obj)
        {
            if (SelectedDoctor == null) return;

            repository.DeleteDoctor(SelectedDoctor.UserId);
            LoadDoctors();
        }

        private bool CanExecute(object obj)
        {
            return SelectedDoctor != null;
        }

        // ================= SEARCH AUTO =================
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
            catch (TaskCanceledException)
            {
            }
        }
    }
}