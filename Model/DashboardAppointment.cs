namespace TamAnh_EMR_System.Model
{
    /// <summary>
    /// Display-oriented model for appointment rows in the dashboard table.
    /// Separate from the domain Appointment model to decouple presentation from data layer.
    /// 
    /// This model contains pre-formatted strings ready for direct display,
    /// making it easy to bind in XAML DataTemplates without converters for basic fields.
    /// 
    /// The Status property drives color-coded badge rendering via StatusToColorConverter
    /// and StatusToBackgroundConverter.
    /// </summary>
    public class DashboardAppointment
    {
        /// <summary>Appointment time (e.g., "08:30")</summary>
        public string Time { get; set; }

        /// <summary>Appointment date (e.g., "THỨ 2, 24/05")</summary>
        public string Date { get; set; }

        /// <summary>Full patient name</summary>
        public string PatientName { get; set; }

        /// <summary>Initials for the avatar circle (e.g., "NL" for Nguyễn Thành Long)</summary>
        public string PatientInitials { get; set; }

        /// <summary>Gender and age info (e.g., "Nam, 32 tuổi")</summary>
        public string GenderAge { get; set; }

        /// <summary>Doctor's full name with title (e.g., "BS. Trần Đức Anh")</summary>
        public string DoctorName { get; set; }

        /// <summary>Department name (e.g., "Khoa Nội")</summary>
        public string Department { get; set; }

        /// <summary>Service/procedure name (e.g., "Khám tổng quát")</summary>
        public string Service { get; set; }

        /// <summary>
        /// Status text that also drives UI color via converters.
        /// Valid values: "Đang khám", "Đang chờ", "Hoàn thành", "Đã hủy"
        /// </summary>
        public string Status { get; set; }

        /// <summary>Background color for the initials avatar circle</summary>
        public string AvatarColor { get; set; }
    }
}
