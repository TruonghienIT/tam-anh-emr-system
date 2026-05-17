using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class AppointmentDisplay
    {
        public string Id { get; set; }

        public string PatientName { get; set; }

        public string DoctorName { get; set; }

        public string Department { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string AppointmentTime { get; set; }

        public string Status { get; set; }

        public string Reason { get; set; }

        public string PatientId { get; set; }

        public string DoctorId { get; set; }
    }
}
