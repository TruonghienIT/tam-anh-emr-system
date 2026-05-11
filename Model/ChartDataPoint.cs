using System;

namespace TamAnh_EMR_System.Model
{
    public class ChartDataPoint
    {
        public string Label { get; set; }

        public double Value { get; set; }

        // SCALE FOR UI
        public double Height
        {
            get
            {
                return Value * 40;
            }
        }
    }
}