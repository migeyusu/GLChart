using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;

namespace GLChart.WPF.UIComponent.Axis;

public class AxisXOption : AxisOption
{
    static AxisXOption()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AxisXOption),
            new FrameworkPropertyMetadata(typeof(AxisXOption)));
    }

    protected override FlowDirection NumberDirection { get; } = FlowDirection.LeftToRight;

    private const FlowDirection Direction = FlowDirection.LeftToRight;

    protected override void RenderScale(DrawingContext context)
    {
        var option = this.LabelRenderOption;
        var emSize = option.FontEmSize;
        var typeface = option.Typeface;
        var foreground = option.Foreground;
        var cultureInfo = option.CultureInfo;
        var axisLabels = AxisLabels;
        foreach (var label in axisLabels)
        {
            var text = new FormattedText(label.Text, cultureInfo, Direction, typeface,
                emSize,
                foreground, 1);
            context.DrawText(
                text, new Point(label.Location - text.Width / 2, 0));
        }

        var renderHeight = option.TextHeight;
        if (!renderHeight.AlmostSame(this.Height))
        {
            this.Height = renderHeight;
        }
    }

    protected override void RefreshLabels(Size size)
    {
        var pixelStart = 0;
        var pixelStretch = size.Width;
        var labelFunc = this.ScaleLabelFunc;
        this.AxisLabels = this.ScaleGenerator
            .Generate(new ScaleLineGenerationContext(this.ViewRange, pixelStart, pixelStretch,
                this.NumberDirection,
                this.LabelRenderOption))
            .Select(scale =>
            {
                var round = Math.Round(scale.Value, this.RoundDigit);
                return new AxisLabel()
                {
                    Location = scale.Location,
                    Value = round,
                    Text = labelFunc.Invoke(round),
                };
            }).ToArray();
    }
}