using System;
using System.ComponentModel;

namespace TamAnh_EMR_System.Model
{
    /// <summary>
    /// Model representing patient registration form data.
    /// Contains all fields from the "Đăng ký & Tiếp nhận bệnh nhân" screen.
    /// 
    /// Implements INotifyPropertyChanged so that individual field changes
    /// propagate to the UI immediately (two-way binding on form inputs).
    /// 
    /// FUTURE: Map this to API request body for backend integration.
    /// </summary>
    public class PatientRegistration : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ======================== THÔNG TIN CÁ NHÂN ========================

        private string _lastName;
        /// <summary>Họ (Last name) - Required</summary>
        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(nameof(LastName)); }
        }

        private string _firstName;
        /// <summary>Tên (First name) - Required</summary>
        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); }
        }

        private DateTime? _dateOfBirth;
        /// <summary>Ngày sinh - Required</summary>
        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set { _dateOfBirth = value; OnPropertyChanged(nameof(DateOfBirth)); }
        }

        private string _gender;
        /// <summary>Giới tính (Nam/Nữ/Khác) - Required</summary>
        public string Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(nameof(Gender)); }
        }

        private string _maritalStatus = "Độc thân";
        /// <summary>Tình trạng hôn nhân: Độc thân / Đã kết hôn / Ly hôn</summary>
        public string MaritalStatus
        {
            get => _maritalStatus;
            set { _maritalStatus = value; OnPropertyChanged(nameof(MaritalStatus)); }
        }

        // ======================== THÔNG TIN LIÊN HỆ ========================

        private string _email;
        /// <summary>Email - Required</summary>
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        private string _phone;
        /// <summary>Số điện thoại - Required</summary>
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(nameof(Phone)); }
        }

        private string _address;
        /// <summary>Địa chỉ (Số nhà, Tên đường) - Required</summary>
        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        private string _city;
        /// <summary>Thành phố</summary>
        public string City
        {
            get => _city;
            set { _city = value; OnPropertyChanged(nameof(City)); }
        }

        private string _district;
        /// <summary>Quận/Huyện</summary>
        public string District
        {
            get => _district;
            set { _district = value; OnPropertyChanged(nameof(District)); }
        }

        private string _postalCode;
        /// <summary>Mã bưu điện</summary>
        public string PostalCode
        {
            get => _postalCode;
            set { _postalCode = value; OnPropertyChanged(nameof(PostalCode)); }
        }
        private string _idCard;
        public string IdCard
        {
            get => _idCard;
            set { _idCard = value; OnPropertyChanged(nameof(IdCard)); }
        }

        private string _bloodType;
        public string BloodType
        {
            get => _bloodType;
            set { _bloodType = value; OnPropertyChanged(nameof(BloodType)); }
        }

        private string _allergies;
        public string Allergies
        {
            get => _allergies;
            set { _allergies = value; OnPropertyChanged(nameof(Allergies)); }
        }

        private string _emergencyContactName;
        public string EmergencyContactName
        {
            get => _emergencyContactName;
            set { _emergencyContactName = value; OnPropertyChanged(nameof(EmergencyContactName)); }
        }

        private string _emergencyContactPhone;
        public string EmergencyContactPhone
        {
            get => _emergencyContactPhone;
            set { _emergencyContactPhone = value; OnPropertyChanged(nameof(EmergencyContactPhone)); }
        }
    }
}
