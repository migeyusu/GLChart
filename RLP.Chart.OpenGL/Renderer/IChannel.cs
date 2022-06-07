using RLP.Chart.Interface.Abstraction;

namespace RLP.Chart.OpenGL.Renderer
{
    public interface IChannel : IGeometry
    {
        IPoint3D[] Points { get; set; }
    }
}