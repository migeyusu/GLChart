using System.Drawing;
using GLChart.WPF.UIComponent;

namespace GLChart.WPF.Base
{
    /// <summary>
    /// 表示线条
    /// </summary>
    public interface ILine2D : ISeries2D<IPoint2D>
    {
        float Thickness { get; set; }
    }
}