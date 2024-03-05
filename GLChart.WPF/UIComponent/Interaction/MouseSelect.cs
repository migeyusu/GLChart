using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GLChart.WPF.UIComponent.Control;

namespace GLChart.WPF.UIComponent.Interaction
{
    public class MouseSelect : FrameworkElement
    {
        public event EventHandler<SelectionArgs> Selected;

        public static readonly DependencyProperty AllowDrawScaleProperty = DependencyProperty.Register(
            "AllowDrawScale", typeof(bool), typeof(Series2DChart), new PropertyMetadata(true));

        public bool AllowDrawScale
        {
            get { return (bool) GetValue(AllowDrawScaleProperty); }
            set { SetValue(AllowDrawScaleProperty, value); }
        }

        public static readonly DependencyProperty MinPointDistanceProperty = DependencyProperty.Register(
            "MinPointDistance", typeof(double), typeof(MouseSelect), new PropertyMetadata(5d));

        public double MinPointDistance
        {
            get { return (double) GetValue(MinPointDistanceProperty); }
            set { SetValue(MinPointDistanceProperty, value); }
        }

        protected DrawingGroup DrawingGroup = new DrawingGroup();

        private bool _canDraw;

        private Point _startPoint;

        public MouseSelect()
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Transparent, 1),
                new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            base.OnRender(drawingContext);
            drawingContext.DrawDrawing(DrawingGroup);
        }

        private readonly Pen _drawPen = new Pen(Brushes.Black, 0.5) {DashStyle = DashStyles.Dot};

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_canDraw)
            {
                var position = e.GetPosition(this);
                using (var drawingContext = DrawingGroup.Open())
                {
                    drawingContext.DrawRectangle(null, _drawPen, new Rect(_startPoint, position));
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(this);
            _canDraw = AllowDrawScale && _startPoint.X > 0 && _startPoint.Y > 0;
            base.OnMouseRightButtonDown(e);
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

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (_canDraw)
            {
                _canDraw = false;
                DrawingGroup.Children.Clear();
                var position = e.GetPosition(this);
                if ((position - _startPoint).Length <= MinPointDistance)
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

                OnSelected(new SelectionArgs()
                {
                    FullRect = new Rect(drawingElementRenderSize),
                    SelectRect = new Rect(_startPoint, position),
                });
            }

            base.OnMouseRightButtonUp(e);
        }

        protected virtual void OnSelected(SelectionArgs e)
        {
            Selected?.Invoke(this, e);
        }
    }
}