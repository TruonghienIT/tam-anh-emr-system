using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class Disease
    {
        public string IcdCode { get; set; }
        public string DiseaseName { get; set; }

        public ICollection<MedicalRecord> MedicalRecords { get; set; }
    }
}
