using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class PrescriptionDetails
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Notes { get; set; }

        public MedicalRecords MedicalRecord { get; set; }
        public Medicines Medicine { get; set; }
    }
}
