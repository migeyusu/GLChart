using System.Drawing;

namespace RLP.Chart.Interface.Abstraction
{
    public interface ILine
    {
        Color LineColor { get; set; }

        int PointCountLimit { get; set; }
    }
}