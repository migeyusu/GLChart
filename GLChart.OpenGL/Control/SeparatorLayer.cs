using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using GLChart.Interface;
using Point = System.Windows.Point;

namespace GLChart.Core.Control
{
    /// <summary>
    /// 绘制划分的图层
    /// </summary>
    public class SeparatorLayer : FrameworkElement
    {
        public static readonly DependencyProperty RegionProperty = DependencyProperty.Register(
            "Region", typeof(Region2D), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(default(Region2D), FrameworkPropertyMetadataOptions.AffectsRender));

        public Region2D Region
        {
            get { return (Region2D)GetValue(RegionProperty); }
            set { SetValue(RegionProperty, value); }
        }

        public static readonly DependencyProperty AxisXOptionProperty = DependencyProperty.Register(
            "AxisXOption", typeof(AxisOption), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(new AxisOption(),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public AxisOption AxisXOption
        {
            get { return (AxisOption)GetValue(AxisXOptionProperty); }
            set { SetValue(AxisXOptionProperty, value); }
        }

        public static readonly DependencyProperty AxisYOptionProperty = DependencyProperty.Register(
            "AxisYOption", typeof(AxisOption), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(new AxisOption(),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public AxisOption AxisYOption
        {
            get { return (AxisOption)GetValue(AxisYOptionProperty); }
            set { SetValue(AxisYOptionProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var region = Region;
            var axisXOption = AxisXOption;
            var axisYOption = AxisYOption;
            var renderSize = this.RenderSize;
            var height = renderSize.Height;
            var width = renderSize.Width;
            if (axisXOption.IsSeparatorVisible)
            {
                var separatePen = axisXOption.SeparatorPen;
                var xAxisLabels =
                    axisXOption.GenerateLabels(0, width, region.XRange, FlowDirection.LeftToRight);
                foreach (var xAxisLabel in xAxisLabels)
                {
                    drawingContext.DrawLine(separatePen, new Point(xAxisLabel.Location, 0),
                        new Point(xAxisLabel.Location, height));
                }
            }

            if (axisYOption.IsSeparatorVisible)
            {
                var separatePen = axisYOption.SeparatorPen;
                var yAxisLabels = axisYOption.GenerateLabels(height, height, region.YRange,
                    FlowDirection.RightToLeft);
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