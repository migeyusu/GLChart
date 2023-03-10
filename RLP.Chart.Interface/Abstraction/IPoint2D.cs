namespace RLP.Chart.Interface.Abstraction
{
    /// <summary>
    /// 二维点，可以通过继承该接口暴露二维信息
    /// </summary>
    public interface IPoint2D : IGeometry
    {
        float X { get; }
        float Y { get; }
    }
}