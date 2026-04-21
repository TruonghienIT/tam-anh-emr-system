using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TamAnh_EMR_System.Model
{
    public class LabResults
    {
        public string Id { get; set; }
        public string RecordId { get; set; }
        public string TestName { get; set; }
        public string Result { get; set; }
        public DateTime TestDate { get; set; }

        public MedicalRecords MedicalRecord { get; set; }
    }
}
