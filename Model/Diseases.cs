using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class Diseases
    {
        public string IcdCode { get; set; }
        public string DiseaseName { get; set; }

        public ICollection<MedicalRecords> MedicalRecords { get; set; }
    }
}
