using System.Windows;
using System.Windows.Media;
using RLP.Chart.Interface;
using RLP.Chart.OpenGL.Collection;
using Point = System.Windows.Point;

namespace RLP.Chart.OpenGL.Control
{
    public class XAxisElement : AxisElement
    {
        protected override void RenderAxis(LabelGenerationOption labelGenerationOption, ScrollRange range,
            DrawingContext context)
        {
            var option = labelGenerationOption.RenderOption;
            var labels =
                labelGenerationOption.GenerateLabels(0, this.RenderSize.Width, range);
            var emSize = option.FontEmSize;
            var typeface = option.Typeface;
            var foreground = option.Foreground;
            var cultureInfo = option.CultureInfo;
            foreach (var label in labels)
            {
                var text = new FormattedText(label.Text, cultureInfo, FlowDirection.LeftToRight, typeface,
                    emSize,
                    foreground, 1);
                context.DrawText(
                    text, new Point(label.Location - text.Width / 2, 0));
            }

            if (AutoSize)
            {
                var renderHeight = option.TextHeight;
                if (!renderHeight.Same(this.Height))
                {
                    this.Height = renderHeight;
                }
            }
        }
    }
}