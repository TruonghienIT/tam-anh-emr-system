using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class MedicalRecords
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string IcdCode { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public Patients Patient { get; set; }
        public Doctors Doctor { get; set; }
        public Diseases Disease { get; set; }

        public ICollection<LabResults> LabResults { get; set; }
        public ICollection<PrescriptionDetails> PrescriptionDetails { get; set; }
    }
}
