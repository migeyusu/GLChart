using System.Drawing;
using RLP.Chart.Interface.Abstraction;

namespace RLP.Chart.OpenGL.Interaction
{
    public class MouseHoverNodeData
    {
        public MouseHoverNodeData(Color color, IPoint2D point, string title)
        {
            Color = color;
            Point = point;
            Title = title;
        }

        public IPoint2D Point { get; }

        public string Title { get; }

        public Color Color { get; }
    }
}