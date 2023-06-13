using System.Windows;
using System.Windows.Media;
using GLChart.Interface;
using GLChart.OpenTK.Collection;
using Point = System.Windows.Point;

namespace GLChart.OpenTK.Control
{
    public class XAxisElement : AxisElement
    {
        protected override void RenderAxis(AxisOption labelGenerationOption, ScrollRange range,
            DrawingContext context)
        {
            var option = labelGenerationOption.RenderOption;
            var labels =
                labelGenerationOption.GenerateLabels(0, this.RenderSize.Width, range, FlowDirection.LeftToRight);
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