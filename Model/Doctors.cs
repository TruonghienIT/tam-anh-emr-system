using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class Doctors
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public Users User { get; set; }
        public ICollection<MedicalRecords> MedicalRecords { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
