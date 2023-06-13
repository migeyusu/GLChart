namespace RLP.Chart.Interface.Abstraction
{
    /// <summary>
    /// 3维点
    /// </summary>
    public interface IPoint3D : IPoint2D
    {
        float Z { get; }
    }
}