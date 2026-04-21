using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class PrescriptionDetails
    {
        public string Id { get; set; }
        public string RecordId { get; set; }
        public string MedicineId { get; set; }
        public int Quantity { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Notes { get; set; }

        public MedicalRecords MedicalRecord { get; set; }
        public Medicines Medicine { get; set; }
    }
}
