using GLChart.WPF.Base;

namespace GLChart.WPF.Render.Renderer
{
    /// <summary>
    /// 通道
    /// </summary>
    public interface IChannel : IGeometry
    {
        IPoint3D[] Points { get; set; }
    }
}