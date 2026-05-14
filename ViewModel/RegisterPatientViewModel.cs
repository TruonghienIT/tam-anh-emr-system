using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TamAnh_EMR_System.Commands;
using TamAnh_EMR_System.Model;
using TamAnh_EMR_System.Repositories;

namespace TamAnh_EMR_System.ViewModel
{
    /// <summary>
    /// ViewModel for the "Đăng ký &amp; Tiếp nhận bệnh nhân" screen.
    /// 
    /// Manages:
    ///   - Patient form data (PatientRegistration model for UI binding)
    ///   - Step navigation (4-step wizard)
    ///   - Form progress tracking
    ///   - Gender/marital status dropdowns
    ///   - Validation (name, phone, email, dob)
    ///   - Save to database via PatientRepository
    ///   - Error/success messaging
    ///   - Form clearing after successful save
    /// 
    /// SAVE FLOW:
    /// User fills form → clicks "Lưu bệnh nhân" → validate all fields →
    /// check duplicate phone → generate BN000001 ID → INSERT into patients →
    /// show success message → clear form
    /// 
    /// All logic stays in ViewModel. No code-behind business logic.
    /// </summary>
    public class RegisterPatientViewModel : ViewModelBase
    {
        // =====================================================================
        // REPOSITORY
        // =====================================================================

        private readonly PatientRepository _patientRepo;

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
        // STATE: Loading, Error, Success
        // =====================================================================

        private bool _isLoading;
        /// <summary>True while save operation is in progress — disables save button</summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }

        /// <summary>Inverted IsLoading for button IsEnabled binding</summary>
        public bool IsNotLoading => !_isLoading;

        private string _errorMessage;
        /// <summary>Validation/save error message displayed in UI</summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        /// <summary>True when there's an error message to display</summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        // =====================================================================
        // POPUP MODE CALLBACK
        // =====================================================================
        
        /// <summary>
        /// Callback action invoked when a patient is successfully saved.
        /// If this is set, the view model assumes it's running inside a popup
        /// and will not auto-clear the form or show a local success message.
        /// </summary>
        public Action<Patients> OnPatientSaved { get; set; }

        private string _successMessage;
        /// <summary>Success message displayed after saving</summary>
        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged(nameof(SuccessMessage));
                OnPropertyChanged(nameof(HasSuccess));
            }
        }

        /// <summary>True when there's a success message to display</summary>
        public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

        private string _savedPatientId;
        /// <summary>The generated patient ID after successful save (e.g., "BN000001")</summary>
        public string SavedPatientId
        {
            get => _savedPatientId;
            set { _savedPatientId = value; OnPropertyChanged(nameof(SavedPatientId)); }
        }

        // =====================================================================
        // COMMANDS
        // All button actions bound via ICommand (RelayCommand).
        // No logic lives in code-behind.
        // =====================================================================

        /// <summary>Validates form and saves patient to database</summary>
        public ICommand SavePatientCommand { get; }

        /// <summary>Clears all form fields and resets state</summary>
        public ICommand ClearFormCommand { get; }

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
            // Initialize repository
            _patientRepo = new PatientRepository();

            // Initialize patient model with empty data
            Patient = new PatientRegistration();

            // Default to first step
            SelectedStep = 0;

            // Gender dropdown options
            GenderOptions = new ObservableCollection<string>
            {
                "Nam",
                "Nữ",
                "Khác"
            };
            BloodTypeOptions = new ObservableCollection<string>
            {
                "A",
                "B",
                "AB",
                "O"
            };

            // Wire up commands
            SavePatientCommand = new RelayCommand(ExecuteSavePatient, CanExecuteSave);
            ClearFormCommand = new RelayCommand(ExecuteClearForm);
            SaveDraftCommand = new RelayCommand(ExecuteSaveDraft);
            PreviousCommand = new RelayCommand(ExecutePrevious, CanExecutePrevious);
            NextCommand = new RelayCommand(ExecuteNext);
        }

        // =====================================================================
        // SAVE PATIENT — Main business logic
        //
        // FLOW:
        // 1. Clear previous messages
        // 2. Validate all form fields
        // 3. Check duplicate phone in DB
        // 4. Map PatientRegistration → Patients (DB model)
        // 5. Call PatientRepository.AddAsync (generates BN000001 ID + INSERT)
        // 6. Show success message with generated ID
        // 7. Clear form for next entry
        //
        // ASYNC: Entire flow is async to keep UI responsive.
        // PatientRepository handles connection/transaction internally.
        // =====================================================================

        private async void ExecuteSavePatient(object parameter)
        {
            // Step 1: Clear previous messages
            ErrorMessage = "";
            SuccessMessage = "";
            SavedPatientId = "";

            // Step 2: Validate all fields
            string validation = ValidateForm();
            if (!string.IsNullOrEmpty(validation))
            {
                ErrorMessage = validation;
                return;
            }

            IsLoading = true;

            try
            {
                // Step 3: Check duplicate phone
                bool phoneExists = await _patientRepo.ExistsByPhoneAsync(Patient.Phone?.Trim());
                if (phoneExists)
                {
                    ErrorMessage = "Số điện thoại này đã được đăng ký cho bệnh nhân khác.";
                    return;
                }

                // Step 4: Map form model → DB model
                var dbPatient = MapToDbModel();

                // Step 5: Save to database (generates BN000001 ID internally)
                var savedPatient = await _patientRepo.AddAsync(dbPatient);

                // Step 6: Show success
                SavedPatientId = savedPatient.Id;
                SuccessMessage = $"Đăng ký thành công! Mã bệnh nhân: {savedPatient.Id}";

                // Step 7: Notify popup or clear form
                if (OnPatientSaved != null)
                {
                    OnPatientSaved.Invoke(savedPatient);
                }
                else
                {
                    // Standalone mode: Clear form after short delay so user can see the message
                    await Task.Delay(1200);
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi lưu dữ liệu: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanExecuteSave(object parameter)
        {
            return !IsLoading;
        }

        // =====================================================================
        // FORM → DB MODEL MAPPING
        //
        // PatientRegistration (form) has LastName + FirstName
        // Patients (DB) has single "name" column
        // Mapping: name = LastName + " " + FirstName (Vietnamese naming convention)
        //
        // Address is composed from Address + City + District
        // =====================================================================

        private Patients MapToDbModel()
        {
            string fullName =
                $"{Patient.LastName?.Trim()} {Patient.FirstName?.Trim()}".Trim();

            string fullAddress = Patient.Address?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(Patient.District))
                fullAddress += ", " + Patient.District.Trim();

            if (!string.IsNullOrWhiteSpace(Patient.City))
                fullAddress += ", " + Patient.City.Trim();

            return new Patients
            {
                Name = fullName,

                Dob = Patient.DateOfBirth ?? DateTime.Now,

                Gender = Patient.Gender,

                Address = fullAddress,

                Phone = Patient.Phone?.Trim(),

                Email = Patient.Email?.Trim(),

                IdCard = Patient.IdCard?.Trim(),

                BloodType = Patient.BloodType,

                Allergies = Patient.Allergies?.Trim(),

                EmergencyContactName = Patient.EmergencyContactName?.Trim(),

                EmergencyContactPhone = Patient.EmergencyContactPhone?.Trim()
            };
        }

        // =====================================================================
        // VALIDATION
        // Checks all required fields and format rules.
        // Returns null if valid, or error message string if invalid.
        // =====================================================================

        private string ValidateForm()
        {
            // Required: Họ (LastName)
            if (string.IsNullOrWhiteSpace(Patient.LastName))
                return "Vui lòng nhập họ bệnh nhân.";

            // Required: Tên (FirstName)
            if (string.IsNullOrWhiteSpace(Patient.FirstName))
                return "Vui lòng nhập tên bệnh nhân.";

            // Required: Ngày sinh
            if (!Patient.DateOfBirth.HasValue)
                return "Vui lòng chọn ngày sinh.";

            // Ngày sinh không lớn hơn hôm nay
            if (Patient.DateOfBirth.Value.Date > DateTime.Today)
                return "Ngày sinh không được lớn hơn ngày hiện tại.";

            // Required: Giới tính
            if (string.IsNullOrWhiteSpace(Patient.Gender))
                return "Vui lòng chọn giới tính.";

            // Required: Email
            if (string.IsNullOrWhiteSpace(Patient.Email))
                return "Vui lòng nhập email.";

            // Email format
            if (!IsValidEmail(Patient.Email))
                return "Email không hợp lệ. Ví dụ: abc@example.com";

            // Required: Phone
            if (string.IsNullOrWhiteSpace(Patient.Phone))
                return "Vui lòng nhập số điện thoại.";

            // Phone format: starts with 0, 10-11 digits
            if (!IsValidPhone(Patient.Phone))
                return "Số điện thoại không hợp lệ. Vui lòng nhập 10-11 số bắt đầu bằng 0.";

            // Required: Địa chỉ
            if (string.IsNullOrWhiteSpace(Patient.Address))
                return "Vui lòng nhập địa chỉ.";

            if (!string.IsNullOrWhiteSpace(Patient.IdCard))
            {
                if (!Regex.IsMatch(Patient.IdCard.Trim(), @"^\d{12}$"))
                    return "CCCD phải gồm đúng 12 số.";
            }

            if (!string.IsNullOrWhiteSpace(Patient.EmergencyContactPhone))
            {
                if (!IsValidPhone(Patient.EmergencyContactPhone))
                    return "SĐT người liên hệ khẩn cấp không hợp lệ.";
            }

            return null; // All valid
        }

        /// <summary>Validates email format using regex</summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email.Trim(),
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        }

        /// <summary>Validates Vietnamese phone format: starts with 0, 10-11 digits</summary>
        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return Regex.IsMatch(phone.Trim(), @"^0\d{9,10}$");
        }

        // =====================================================================
        // CLEAR FORM
        // Resets all form fields and state to initial values
        // =====================================================================

        private void ExecuteClearForm(object parameter)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            Patient = new PatientRegistration();

            SelectedStep = 0;

            ErrorMessage = "";

            SuccessMessage = "";

            SavedPatientId = "";

            NotifyMaritalChanged();
        }

        // =====================================================================
        // STEP NAVIGATION COMMANDS
        // =====================================================================

        private void ExecuteSaveDraft(object parameter)
        {
            // TODO: Implement draft saving logic
            MessageBox.Show("Đã lưu bản nháp thành công!", "Lưu bản nháp",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecutePrevious(object parameter)
        {
            if (SelectedStep > 0)
            {
                SelectedStep--;
            }
        }

        private bool CanExecutePrevious(object parameter)
        {
            return SelectedStep > 0;
        }

        private void ExecuteNext(object parameter)
        {
            if (SelectedStep < 3)
            {
                SelectedStep++;
            }
            else
            {
                // Final step — trigger save
                ExecuteSavePatient(null);
            }
        }
        public ObservableCollection<string> BloodTypeOptions { get; set; }
    }
}
