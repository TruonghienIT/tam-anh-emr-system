namespace TamAnh_EMR_System.Model
{
    public class AppointmentQrDto
    {
        public string appointmentId { get; set; }
        public string patientName { get; set; }
        public string phoneNumber { get; set; }
        public string doctorName { get; set; }
        public string department { get; set; }
        public string appointmentDate { get; set; }
        public string appointmentTime { get; set; }
        public string status { get; set; }
        public string reason { get; set; }
    }
}