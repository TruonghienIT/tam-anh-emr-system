using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int? CreatedBy { get; set; }

        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public Receptionist Receptionist { get; set; }
    }
}
