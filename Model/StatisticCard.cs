using System.Windows.Media;

namespace TamAnh_EMR_System.Model
{
    /// <summary>
    /// Represents a statistics summary card on the dashboard.
    /// Each card shows a KPI metric (e.g., total appointments, waiting count).
    /// 
    /// Bound to an ItemsControl in StatisticCardControl to render 4 cards dynamically.
    /// Properties like ValueColor and ProgressColor allow each card to have
    /// its own accent color matching the design.
    /// </summary>
    public class StatisticCard
    {
        /// <summary>Title label (e.g., "TỔNG LỊCH HẸN")</summary>
        public string Title { get; set; }

        /// <summary>Main numeric value (e.g., "42")</summary>
        public string Value { get; set; }

        /// <summary>Supplementary text below the value (e.g., "↑12%", "Hiện tại", "/42")</summary>
        public string SubText { get; set; }

        /// <summary>Color of the main value text</summary>
        public Brush ValueColor { get; set; }

        /// <summary>Color of the sub-text</summary>
        public Brush SubTextColor { get; set; }

        /// <summary>Progress bar fill ratio (0.0 to 1.0), used for cards with progress indicators</summary>
        public double ProgressValue { get; set; }

        /// <summary>Color of the progress bar</summary>
        public Brush ProgressColor { get; set; }

        /// <summary>Whether to show the progress bar (some cards show percentage text instead)</summary>
        public bool ShowProgress { get; set; }
    }
}
