namespace RLP.Chart.Interface.Abstraction
{
    public interface ILineChart : ICoordinate2DChart, ISeriesChart<ILineSeries>
    {
        /// <summary>
        /// 自适应Y轴
        /// </summary>
        bool AutoYAxis { set; }

        double LineThickness { set; }
    }
}