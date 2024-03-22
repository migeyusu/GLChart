using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render;
using GLChart.WPF.UIComponent.Axis;
using GLChart.WPF.UIComponent.Control;

namespace GLChart.WPF.UIComponent.Interaction
{
    public class MouseInteractionElement : FrameworkElement
    {
        public event EventHandler<SelectionArgs> Selected;

        public static readonly DependencyProperty AllowDrawScaleProperty = DependencyProperty.Register(
            "AllowDrawScale", typeof(bool), typeof(Chart2DCore), new PropertyMetadata(true));

        public bool AllowDrawScale
        {
            get { return (bool)GetValue(AllowDrawScaleProperty); }
            set { SetValue(AllowDrawScaleProperty, value); }
        }

        public static readonly DependencyProperty MinPointDistanceProperty = DependencyProperty.Register(
            "MinPointDistance", typeof(double), typeof(MouseInteractionElement), new PropertyMetadata(5d));

        public double MinPointDistance
        {
            get { return (double)GetValue(MinPointDistanceProperty); }
            set { SetValue(MinPointDistanceProperty, value); }
        }

        public static readonly DependencyProperty AxisXOptionProperty = DependencyProperty.Register(
            nameof(AxisXOption), typeof(AxisXOption), typeof(MouseInteractionElement),
            new FrameworkPropertyMetadata(default));

        public AxisXOption AxisXOption
        {
            get => (AxisXOption)GetValue(AxisXOptionProperty);
            set => SetValue(AxisXOptionProperty, value);
        }

        public static readonly DependencyProperty AxisYOptionProperty = DependencyProperty.Register(
            nameof(AxisYOption), typeof(AxisYOption), typeof(MouseInteractionElement),
            new FrameworkPropertyMetadata(default));

        public AxisYOption AxisYOption
        {
            get => (AxisYOption)GetValue(AxisYOptionProperty);
            set => SetValue(AxisYOptionProperty, value);
        }

        /// <summary>
        /// 当前实际视域
        /// </summary>
        public Region2D ActualRegion
        {
            get { return new Region2D(AxisXOption.ViewRange, AxisYOption.ActualViewRange); }
        }

        protected DrawingGroup DrawingGroup = new DrawingGroup();

        private bool _canDraw;


        public MouseInteractionElement()
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Transparent, 1),
                new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            base.OnRender(drawingContext);
            drawingContext.DrawDrawing(DrawingGroup);
        }

        private readonly Pen _drawPen = new Pen(Brushes.Black, 0.5) { DashStyle = DashStyles.Dot };

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            if (_canDraw)
            {
                using (var drawingContext = DrawingGroup.Open())
                {
                    drawingContext.DrawRectangle(null, _drawPen, new Rect(_startMovePoint, position));
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                var winToGlMapping =
                    new WindowsGlCoordinateMapping(this.ActualRegion, new Rect(this.RenderSize));
                var xPixel = _startMovePoint.X - position.X;
                var xOffset = winToGlMapping.GetXOffset(xPixel);
                var yPixel = _startMovePoint.Y - position.Y;
                var yOffset = winToGlMapping.GetYOffset(-yPixel);
                var @new = _startMoveView.OffsetNew(xOffset, yOffset);
                AxisXOption.TryMoveView(@new.XRange);
                if (!AxisYOption.IsAutoSize)
                {
                    AxisYOption.TryMoveView(@new.YRange);
                }
            }

            base.OnMouseMove(e);
        }


        private Point _startMovePoint;

        private Region2D _startMoveView;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _startMovePoint = e.GetPosition(this);
            _startMoveView = this.ActualRegion;
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            _startMovePoint = e.GetPosition(this);
            _canDraw = AllowDrawScale && _startMovePoint.X > 0 && _startMovePoint.Y > 0;
            base.OnMouseRightButtonDown(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (_canDraw)
            {
                _canDraw = false;
                DrawingGroup.Children.Clear();
                var position = e.GetPosition(this);
                if ((position - _startMovePoint).Length <= MinPointDistance)
                {
                    return;
                }

                var drawingElementRenderSize = this.RenderSize;
                if (position.X < 0)
                {
                    position.X = 0;
                }
                else if (position.X > drawingElementRenderSize.Width)
                {
                    position.X = drawingElementRenderSize.Width;
                }

                if (position.Y < 0)
                {
                    position.Y = 0;
                }
                else if (position.Y > drawingElementRenderSize.Height)
                {
                    position.Y = drawingElementRenderSize.Height;
                }

                var scale = new WindowsGlCoordinateMapping(this.ActualRegion, new Rect(drawingElementRenderSize));
                scale.ScaleByRect(new Rect(_startMovePoint, position), out var xRange, out var yRange);
                AxisXOption.TryScaleView(xRange);
                if (!AxisYOption.IsAutoSize)
                {
                    AxisYOption.TryScaleView(yRange);
                }
            }

            base.OnMouseRightButtonUp(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (_canDraw)
            {
                _canDraw = false;
                DrawingGroup.Children.Clear();
            }

            base.OnMouseLeave(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            var axisXOption = AxisXOption;
            var region = this.ActualRegion;
            var newXRange = region.XRange;
            var newYRange = region.YRange;
            if (axisXOption.ZoomEnable)
            {
                newXRange = e.Scale(newXRange);
                axisXOption.TryScaleView(newXRange);
            }

            var axisYOption = AxisYOption;
            if (axisYOption.ZoomEnable && !axisYOption.IsAutoSize)
            {
                newYRange = e.Scale(newYRange);
                axisYOption.TryScaleView(newYRange);
            }
        }
    }
}