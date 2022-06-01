using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using RLP.Chart.Interface;
using RLP.Chart.OpenGL.Interaction;
using Point = System.Windows.Point;

namespace RLP.Chart.OpenGL.Control
{
    /// <summary>
    /// 绘制划分的图层
    /// </summary>
    public class SeparatorLayer : FrameworkElement
    {
        public static readonly DependencyProperty OptionProperty = DependencyProperty.Register(
            "Option", typeof(SeparatorOption), typeof(SeparatorLayer), new PropertyMetadata(new SeparatorOption()));

        public SeparatorOption Option
        {
            get { return (SeparatorOption)GetValue(OptionProperty); }
            set { SetValue(OptionProperty, value); }
        }


        public static readonly DependencyProperty CoordinateRegionProperty = DependencyProperty.Register(
            "CoordinateRegion", typeof(Region2D), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(default(Region2D), FrameworkPropertyMetadataOptions.AffectsRender));
        public Region2D CoordinateRegion
        {
            get { return (Region2D)GetValue(CoordinateRegionProperty); }
            set { SetValue(CoordinateRegionProperty, value); }
        }

        public static readonly DependencyProperty AxisXGenerationOptionProperty = DependencyProperty.Register(
            "AxisXGenerationOption", typeof(LabelGenerationOption), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(LabelGenerationOption.Default,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public LabelGenerationOption AxisXGenerationOption
        {
            get { return (LabelGenerationOption)GetValue(AxisXGenerationOptionProperty); }
            set { SetValue(AxisXGenerationOptionProperty, value); }
        }

        public static readonly DependencyProperty AxisYGenerationOptionProperty = DependencyProperty.Register(
            "AxisYGenerationOption", typeof(LabelGenerationOption), typeof(SeparatorLayer),
            new FrameworkPropertyMetadata(LabelGenerationOption.Default,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public LabelGenerationOption AxisYGenerationOption
        {
            get { return (LabelGenerationOption)GetValue(AxisYGenerationOptionProperty); }
            set { SetValue(AxisYGenerationOptionProperty, value); }
        }


        private Pen _separatePen;

        private bool _showXAxis, _showYAxis;

        public SeparatorLayer()
        {
            void Refresh()
            {
                _separatePen = this.Option.Pen;
                _showXAxis = Option.ShowXAxis;
                _showYAxis = Option.ShowYAxis;
            }

            Refresh();
            DependencyPropertyDescriptor.FromProperty(OptionProperty, typeof(SeparatorLayer))
                .AddValueChanged(this,
                    ((sender, args) => { Refresh(); }));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var coordinateRegion = CoordinateRegion;
            var coordinateRegionXRange = coordinateRegion.XRange;
            var coordinateRegionYRange = coordinateRegion.YRange;
            var axisXGenerationOption = AxisXGenerationOption;
            var axisYGenerationOption = AxisYGenerationOption;
            var renderSize = this.RenderSize;
            var height = renderSize.Height;
            var width = renderSize.Width;
            if (_showXAxis)
            {
                var xAxisLabels = axisXGenerationOption.GenerateLabels(0, width, coordinateRegionXRange);
                foreach (var xAxisLabel in xAxisLabels)
                {
                    drawingContext.DrawLine(_separatePen, new Point(xAxisLabel.Location, 0),
                        new Point(xAxisLabel.Location, height));
                }
            }

            if (_showYAxis)
            {
                var yAxisLabels = axisYGenerationOption.GenerateLabels(height, height, coordinateRegionYRange);
                foreach (var yAxisLabel in yAxisLabels)
                {
                    drawingContext.DrawLine(_separatePen, new Point(0, yAxisLabel.Location),
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