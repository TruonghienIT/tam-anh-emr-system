using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;

namespace TamAnh_EMR_System.ViewModel
{
    /// <summary>
    /// ViewModel for the "Đăng ký & Tiếp nhận bệnh nhân" screen.
    /// 
    /// Manages:
    ///   - Patient form data (PatientRegistration model)
    ///   - Step navigation (4-step wizard)
    ///   - Form progress tracking
    ///   - Gender dropdown options
    ///   - Marital status selection
    ///   - Save/Navigate/Submit commands
    /// 
    /// All form fields bind to the Patient property.
    /// Commands handle button actions without any code-behind logic.
    /// 
    /// FUTURE: Inject a service/repository to persist data via API.
    /// </summary>
    public class RegisterPatientViewModel : ViewModelBase
    {
        // =====================================================================
        // PATIENT DATA
        // The main form model — all input fields bind to Patient.PropertyName
        // =====================================================================

        private PatientRegistration _patient;
        /// <summary>Patient registration form data bound to all input fields</summary>
        public PatientRegistration Patient
        {
            get => _patient;
            set { _patient = value; OnPropertyChanged(nameof(Patient)); }
        }

        // =====================================================================
        // STEP NAVIGATION
        // Controls the 4-step wizard on the left side
        // =====================================================================

        private int _selectedStep;
        /// <summary>
        /// Currently active step (0-based index).
        /// 0 = Thông tin cá nhân, 1 = Thông tin liên hệ,
        /// 2 = Người liên hệ khẩn cấp, 3 = Thông tin bảo hiểm
        /// </summary>
        public int SelectedStep
        {
            get => _selectedStep;
            set { _selectedStep = value; OnPropertyChanged(nameof(SelectedStep)); }
        }

        // =====================================================================
        // PROGRESS TRACKING
        // Shows completion percentage in the header area
        // =====================================================================

        private int _progressPercent;
        /// <summary>Form completion percentage (0-100)</summary>
        public int ProgressPercent
        {
            get => _progressPercent;
            set { _progressPercent = value; OnPropertyChanged(nameof(ProgressPercent)); }
        }

        // =====================================================================
        // DROPDOWN OPTIONS
        // Data sources for ComboBox controls
        // =====================================================================

        /// <summary>Gender options for the dropdown</summary>
        public ObservableCollection<string> GenderOptions { get; set; }

        // =====================================================================
        // MARITAL STATUS
        // Bound to radio-style toggle buttons
        // =====================================================================

        /// <summary>Whether "Độc thân" is selected</summary>
        public bool IsSingle
        {
            get => Patient?.MaritalStatus == "Độc thân";
            set { if (value) { Patient.MaritalStatus = "Độc thân"; NotifyMaritalChanged(); } }
        }

        /// <summary>Whether "Đã kết hôn" is selected</summary>
        public bool IsMarried
        {
            get => Patient?.MaritalStatus == "Đã kết hôn";
            set { if (value) { Patient.MaritalStatus = "Đã kết hôn"; NotifyMaritalChanged(); } }
        }

        /// <summary>Whether "Ly hôn" is selected</summary>
        public bool IsDivorced
        {
            get => Patient?.MaritalStatus == "Ly hôn";
            set { if (value) { Patient.MaritalStatus = "Ly hôn"; NotifyMaritalChanged(); } }
        }

        private void NotifyMaritalChanged()
        {
            OnPropertyChanged(nameof(IsSingle));
            OnPropertyChanged(nameof(IsMarried));
            OnPropertyChanged(nameof(IsDivorced));
        }

        // =====================================================================
        // COMMANDS
        // All button actions bound via ICommand (RelayCommand).
        // No logic lives in code-behind.
        // =====================================================================

        /// <summary>Saves current form data as draft</summary>
        public ICommand SaveDraftCommand { get; }

        /// <summary>Navigates to the previous step</summary>
        public ICommand PreviousCommand { get; }

        /// <summary>Validates and moves to the next step</summary>
        public ICommand NextCommand { get; }

        // =====================================================================
        // CONSTRUCTOR
        // =====================================================================

        public RegisterPatientViewModel()
        {
            // Initialize patient model with empty data
            Patient = new PatientRegistration();

            // Default to first step
            SelectedStep = 0;
            ProgressPercent = 75;

            // Gender dropdown options
            GenderOptions = new ObservableCollection<string>
            {
                "Nam",
                "Nữ",
                "Khác"
            };

            // Wire up commands
            SaveDraftCommand = new RelayCommand(ExecuteSaveDraft);
            PreviousCommand = new RelayCommand(ExecutePrevious, CanExecutePrevious);
            NextCommand = new RelayCommand(ExecuteNext);
        }

        // =====================================================================
        // COMMAND IMPLEMENTATIONS
        // Placeholder logic — ready for backend integration
        // =====================================================================

        private void ExecuteSaveDraft(object parameter)
        {
            // TODO: Call API to save as draft
            MessageBox.Show("Đã lưu bản nháp thành công!", "Lưu bản nháp",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecutePrevious(object parameter)
        {
            if (SelectedStep > 0)
            {
                SelectedStep--;
                UpdateProgress();
            }
        }

        private bool CanExecutePrevious(object parameter)
        {
            return SelectedStep > 0;
        }

        private void ExecuteNext(object parameter)
        {
            // TODO: Add validation before proceeding
            if (SelectedStep < 3)
            {
                SelectedStep++;
                UpdateProgress();
            }
            else
            {
                // Final step — submit registration
                MessageBox.Show("Xác minh và tiếp tục đăng ký!", "Xác minh",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>Updates progress based on current step</summary>
        private void UpdateProgress()
        {
            ProgressPercent = (SelectedStep + 1) * 25;
        }
    }
}
