
using System.Windows.Media;
using GLChart.WPF.Base;

namespace GLChart.WPF.UIComponent.Interaction
{
    public class MouseHoverNodeData
    {
        public MouseHoverNodeData(Color color, IGeometry point, string title)
        {
            Color = color;
            Point = point;
            Title = title;
        }

        public IGeometry Point { get; }

        public string Title { get; }

        public Color Color { get; }
    }
}