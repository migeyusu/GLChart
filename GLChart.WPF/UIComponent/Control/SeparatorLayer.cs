using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Render;
using GLChart.WPF.UIComponent.Axis;
using Point = System.Windows.Point;

namespace GLChart.WPF.UIComponent.Control
{
    /// <summary>
    /// 绘制划分的图层
    /// </summary>
    public class SeparatorLayer : FrameworkElement
    {
        public static readonly DependencyProperty AxisXOptionProperty = DependencyProperty.Register(
            "AxisXOption", typeof(AxisXOption), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(default,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public AxisXOption AxisXOption
        {
            get { return (AxisXOption)GetValue(AxisXOptionProperty); }
            set { SetValue(AxisXOptionProperty, value); }
        }

        public static readonly DependencyProperty AxisYOptionProperty = DependencyProperty.Register(
            "AxisYOption", typeof(AxisYOption), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(default,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public AxisYOption AxisYOption
        {
            get { return (AxisYOption)GetValue(AxisYOptionProperty); }
            set { SetValue(AxisYOptionProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var axisXOption = AxisXOption;
            var axisYOption = AxisYOption;
            var renderSize = this.RenderSize;
            var height = renderSize.Height;
            var width = renderSize.Width;
            if (axisXOption?.IsSeparatorVisible == true)
            {
                var separatePen = axisXOption.SeparatorPen;
                var xAxisLabels = axisXOption.GenerateLabels(0, width);
                foreach (var xAxisLabel in xAxisLabels)
                {
                    drawingContext.DrawLine(separatePen, new Point(xAxisLabel.Location, 0),
                        new Point(xAxisLabel.Location, height));
                }
            }

            if (axisYOption?.IsSeparatorVisible == true)
            {
                var separatePen = axisYOption.SeparatorPen;
                var yAxisLabels = axisYOption.GenerateLabels(height, height);
                foreach (var yAxisLabel in yAxisLabels)
                {
                    drawingContext.DrawLine(separatePen, new Point(0, yAxisLabel.Location),
                        new Point(width, yAxisLabel.Location));
                }
            }
        }
    }

    public class SquareAxisLabels
    {
        public SquareAxisLabels(IEnumerable<AxisLabel> xLabels, IEnumerable<AxisLabel> yLabels)
        {
            YLabels = yLabels;
            XLabels = xLabels;
        }

        public IEnumerable<AxisLabel> XLabels { get; }
        public IEnumerable<AxisLabel> YLabels { get; }
    }
}