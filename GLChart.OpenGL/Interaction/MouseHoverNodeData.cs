using System.Drawing;
using GLChart.Interface.Abstraction;

namespace GLChart.OpenTK.Interaction
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