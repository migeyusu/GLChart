namespace GLChart.WPF.Base
{
    /// <summary>
    /// 二维点，可以通过继承该接口暴露二维信息
    /// </summary>
    public interface IPoint2D : IGeometry2D
    {
        float X { get; }
        float Y { get; }
    }
}