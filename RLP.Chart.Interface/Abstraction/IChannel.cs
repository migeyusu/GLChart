namespace RLP.Chart.Interface.Abstraction
{
    /// <summary>
    /// 通道
    /// </summary>
    public interface IChannel : IGeometry
    {
        IPoint3D[] Points { get; set; }
    }
}