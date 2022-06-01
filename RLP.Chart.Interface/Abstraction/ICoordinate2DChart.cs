namespace RLP.Chart.Interface.Abstraction
{
    /// <summary>
    /// 基于2d坐标系的图表
    /// </summary>
    public interface ICoordinate2DChart
    {
        /// <summary>
        /// 当自动Y轴时，返回和设定会不同，即返回actual display region
        /// 如果非自动Y轴时返回和设定立即相同
        /// <para>注意这是接口实现的强制标准</para>
        /// </summary>
        Region2D DisplayRegion { get; set; }
    }
}