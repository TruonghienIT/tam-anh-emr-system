namespace TamAnh_EMR_System.Model
{
    /// <summary>
    /// Represents a single data point in the dashboard bar chart 
    /// ("Mật độ lịch khám 24h tới").
    /// 
    /// Each point has two values for the dual-bar display:
    /// - Value1: primary bar (darker blue)
    /// - Value2: secondary bar (lighter blue)
    /// 
    /// The Label shows the hour label (e.g., "08H", "09H").
    /// MaxValue is used to calculate bar heights as proportional percentages.
    /// </summary>
    public class ChartDataPoint
    {
        /// <summary>Time label displayed below the bar (e.g., "08H")</summary>
        public string Label { get; set; }

        /// <summary>Primary bar value (darker blue)</summary>
        public double Value1 { get; set; }

        /// <summary>Secondary bar value (lighter blue)</summary>
        public double Value2 { get; set; }

        /// <summary>Maximum possible value for scaling bar heights</summary>
        public double MaxValue { get; set; }

        /// <summary>Calculated height for primary bar (as percentage of max, scaled to pixels)</summary>
        public double Bar1Height => MaxValue > 0 ? (Value1 / MaxValue) * 120 : 0;

        /// <summary>Calculated height for secondary bar</summary>
        public double Bar2Height => MaxValue > 0 ? (Value2 / MaxValue) * 120 : 0;
    }
}
