using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class Appointment
    {
        public string Id { get; set; }
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public string CreatedBy { get; set; }

        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }

        public Patients Patient { get; set; }
        public Doctors Doctor { get; set; }
        public Receptionists Receptionist { get; set; }
    }
}
