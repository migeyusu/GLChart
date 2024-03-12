﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;

namespace GLChart.WPF.UIComponent.Axis;

public class AxisYOption : AxisOption
{
    /// <summary>
    /// 是否自适应高度
    /// </summary>
    public static readonly DependencyProperty IsAutoSizeProperty = DependencyProperty.Register(
        nameof(IsAutoSize), typeof(bool), typeof(AxisYOption), new PropertyMetadata(default(bool)));

    /// <summary>
    /// 是否自适应高度
    /// </summary>
    public bool IsAutoSize
    {
        get { return (bool)GetValue(IsAutoSizeProperty); }
        set { SetValue(IsAutoSizeProperty, value); }
    }

    public static readonly DependencyProperty ActualViewRangeProperty = DependencyProperty.Register(
        nameof(ActualViewRange), typeof(ScrollRange), typeof(AxisYOption),
        new FrameworkPropertyMetadata(default(ScrollRange), RefreshLabelsPropertyChangedCallback));

    /// <summary>
    /// 实际视图区域，当启用<see cref="IsAutoSize"/>时，该值将不同于<see cref="AxisOption.ViewRange"/>
    /// </summary>
    public ScrollRange ActualViewRange
    {
        get { return (ScrollRange)GetValue(ActualViewRangeProperty); }
        set { SetValue(ActualViewRangeProperty, value); }
    }

    protected override FlowDirection NumberDirection { get; } = FlowDirection.RightToLeft;

    private const FlowDirection Direction = FlowDirection.LeftToRight;

    protected override void RenderScale(DrawingContext context)
    {
        var scrollRange = this.ActualViewRange;
        if (scrollRange.IsEmpty())
        {
            return;
        }

        var option = this.LabelRenderOption;
        var fontSize = option.FontEmSize;
        var typeface = option.Typeface;
        var cultureInfo = option.CultureInfo;
        var foreground = option.Foreground;
        var axisLabels = AxisLabels;
        double maxWidth = 0;
        foreach (var label in axisLabels)
        {
            var text = new FormattedText(label.Text, cultureInfo, Direction, typeface, fontSize,
                foreground, 1);
            var textHeight = text.Height;
            var textWidth = text.Width;
            if (maxWidth < textWidth)
            {
                maxWidth = textWidth;
            }

            context.DrawText(text
                , new Point(0, label.Location - textHeight / 2));
        }

        if (!maxWidth.AlmostSame(this.Width))
        {
            this.Width = maxWidth;
        }
    }

    protected override void RefreshLabels(Size size)
    {
        var height = size.Height;
        var pixelStart = height;
        var pixelStretch = height;
        var labelFunc = this.ScaleLabelFunc;
        this.AxisLabels = this.ScaleGenerator
            .Generate(new ScaleLineGenerationContext(this.ActualViewRange, pixelStart, pixelStretch,
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