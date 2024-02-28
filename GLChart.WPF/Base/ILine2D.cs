using System.Drawing;

namespace GLChart.WPF.Base
{
    /// <summary>
    /// 表示线条
    /// </summary>
    public interface ILine2D : IGeometry, IGeometryCollection<IPoint2D>
    {
        Color LineColor { get; set; }
     
        string Title { get; set; }
        
        float Thickness { get; set; }
    }
}