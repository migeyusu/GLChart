using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render;
using GLChart.WPF.Render.Allocation;
using GLChart.WPF.UIComponent.Axis;
using GLChart.WPF.UIComponent.Control;

namespace GLChart.WPF.UIComponent
{
    public class YAxisElement : AxisElement
    {
        protected override void RenderAxis(AxisOption labelGenerationOption, ScrollRange range,
            DrawingContext context)
        {
            var option = labelGenerationOption.RenderOption;
            var height = this.RenderSize.Height;
            var labels =
                labelGenerationOption.GenerateLabels(height, height, range, FlowDirection.RightToLeft);
            var fontSize = option.FontEmSize;
            var typeface = option.Typeface;
            var cultureInfo = option.CultureInfo;
            var foreground = option.Foreground;
            double maxWidth = 0;
            foreach (var label in labels)
            {
                var text = new FormattedText(label.Text, cultureInfo, FlowDirection.LeftToRight, typeface, fontSize,
                    foreground, 1);
                var textHeight = text.Height;
                var textWidth = text.Width;
                if (maxWidth < textWidth)
                {
                    maxWidth = textWidth;
                }

                context.DrawText(text
                    , new System.Windows.Point(0, label.Location - textHeight / 2));
            }

            if (AutoSize)
            {
                if (!maxWidth.Same(this.Width))
                {
                    this.Width = maxWidth;
                }
            }
        }
    }
}