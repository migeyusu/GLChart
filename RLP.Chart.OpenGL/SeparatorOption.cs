using System.Windows.Media;

namespace RLP.Chart.OpenGL
{
    public class SeparatorOption
    {
        public Pen Pen { get; set; }

        public bool ShowYAxis { get; set; }

        public bool ShowXAxis { get; set; }

        public SeparatorOption()
        {
            this.Pen = new Pen(Brushes.Gray, 0.5d)
                { DashStyle = DashStyles.DashDotDot /*new DashStyle(new double[] { 10, 10, 1, 10 }, 0)*/ };
            this.ShowYAxis = true;
            this.ShowXAxis = true;
        }
    }
}