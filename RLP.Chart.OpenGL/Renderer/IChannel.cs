using RLP.Chart.Interface.Abstraction;

namespace RLP.Chart.OpenGL.Renderer
{
    //todo:移除接口
    /// <summary>
    /// 通道
    /// </summary>
    public interface IChannel : IGeometry
    {
        IPoint3D[] Points { get; set; }
    }
}