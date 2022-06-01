using RLP.Chart.Interface.Abstraction;

namespace RLP.Chart.OpenGL.Abstraction
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILineRenderer : IShaderRendererItem, IGeometryRenderer<IPoint2D>, ILine
    {
    }
}