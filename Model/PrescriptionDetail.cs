using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class PrescriptionDetail
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Notes { get; set; }

        public MedicalRecord MedicalRecord { get; set; }
        public Medicine Medicine { get; set; }
    }
}
