using System.Drawing;

namespace RLP.Chart.Interface.Abstraction
{
    /// <summary>
    /// 表示线条
    /// </summary>
    public interface ILine : IGeometry, IGeometryCollection<IPoint2D>
    {
        Color LineColor { get; set; }

        int PointCountLimit { get; set; }
    }
}