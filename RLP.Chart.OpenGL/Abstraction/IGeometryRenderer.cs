using RLP.Chart.Interface.Abstraction;

namespace RLP.Chart.OpenGL.Abstraction
{
    public interface IGeometryRenderer<T> : IGeometryCollection<T> where T : IGeometry
    {
    }
}