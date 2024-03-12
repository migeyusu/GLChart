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
        public static readonly DependencyProperty AxisXLabelsProperty = DependencyProperty.Register(
            nameof(AxisXLabels), typeof(IEnumerable<AxisLabel>), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(default(IEnumerable<AxisLabel>?),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public IEnumerable<AxisLabel>? AxisXLabels
        {
            get { return (IEnumerable<AxisLabel>?)GetValue(AxisXLabelsProperty); }
            set { SetValue(AxisXLabelsProperty, value); }
        }

        public static readonly DependencyProperty AxisYLabelsProperty = DependencyProperty.Register(
            nameof(AxisYLabels), typeof(IEnumerable<AxisLabel>), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(default(IEnumerable<AxisLabel>?),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public IEnumerable<AxisLabel>? AxisYLabels
        {
            get { return (IEnumerable<AxisLabel>?)GetValue(AxisYLabelsProperty); }
            set { SetValue(AxisYLabelsProperty, value); }
        }

        public static readonly DependencyProperty IsXSeparatorVisibleProperty = DependencyProperty.Register(
            nameof(IsXSeparatorVisible), typeof(bool), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsXSeparatorVisible
        {
            get { return (bool)GetValue(IsXSeparatorVisibleProperty); }
            set { SetValue(IsXSeparatorVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsYSeparatorVisibleProperty = DependencyProperty.Register(
            nameof(IsYSeparatorVisible), typeof(bool), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsYSeparatorVisible
        {
            get { return (bool)GetValue(IsYSeparatorVisibleProperty); }
            set { SetValue(IsYSeparatorVisibleProperty, value); }
        }

        public static readonly DependencyProperty XSeparatorPenProperty = DependencyProperty.Register(
            nameof(XSeparatorPen), typeof(Pen), typeof(SeparatorLayer), new FrameworkPropertyMetadata(
                new Pen(Brushes.Gray, 0.5d)
                    { DashStyle = DashStyles.DashDotDot }, FrameworkPropertyMetadataOptions.AffectsRender));

        public Pen XSeparatorPen
        {
            get { return (Pen)GetValue(XSeparatorPenProperty); }
            set { SetValue(XSeparatorPenProperty, value); }
        }

        public static readonly DependencyProperty YSeparatorPenProperty = DependencyProperty.Register(
            nameof(YSeparatorPen), typeof(Pen), typeof(SeparatorLayer), new FrameworkPropertyMetadata(
                new Pen(Brushes.Gray, 0.5d)
                    { DashStyle = DashStyles.DashDotDot }, FrameworkPropertyMetadataOptions.AffectsRender));

        public Pen YSeparatorPen
        {
            get { return (Pen)GetValue(YSeparatorPenProperty); }
            set { SetValue(YSeparatorPenProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var renderSize = this.RenderSize;
            var height = renderSize.Height;
            var width = renderSize.Width;
            if (IsXSeparatorVisible)
            {
                var xAxisLabels = AxisXLabels;
                if (xAxisLabels != null)
                {
                    var separatePen = XSeparatorPen;
                    foreach (var xAxisLabel in xAxisLabels)
                    {
                        drawingContext.DrawLine(separatePen, new Point(xAxisLabel.Location, 0),
                            new Point(xAxisLabel.Location, height));
                    }
                }
            }

            if (IsYSeparatorVisible)
            {
                var yAxisLabels = AxisYLabels;
                if (yAxisLabels != null)
                {
                    var separatePen = YSeparatorPen;
                    foreach (var yAxisLabel in yAxisLabels)
                    {
                        drawingContext.DrawLine(separatePen, new Point(0, yAxisLabel.Location),
                            new Point(width, yAxisLabel.Location));
                    }
                }
            }
        }
    }
}