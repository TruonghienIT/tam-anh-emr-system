using System;
using System.Collections.Generic;

namespace TamAnh_EMR_System.Model
{
    public class PrescriptionGroup
    {
        public string RecordId { get; set; }

        public DateTime CreatedAt { get; set; }

        public MedicalRecords MedicalRecord { get; set; }

        public List<PrescriptionDetails> PrescriptionDetails { get; set; }
            = new List<PrescriptionDetails>();
    }
}