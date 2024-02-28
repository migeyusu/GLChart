using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GLChart.WPF.UIComponent.Control;
using Microsoft.Xaml.Behaviors;

namespace GLChart.WPF.UIComponent.Interaction
{
    public class SelectScaleAttach : Behavior<UIElement>
    {
        public event EventHandler<SelectionArgs> Scaled;

        public static readonly DependencyProperty MinPointDistanceProperty = DependencyProperty.Register(
            "MinPointDistance", typeof(double), typeof(SelectScaleAttach), new PropertyMetadata(5d));

        public double MinPointDistance
        {
            get { return (double) GetValue(MinPointDistanceProperty); }
            set { SetValue(MinPointDistanceProperty, value); }
        }

        public static readonly DependencyProperty DrawableElementProperty = DependencyProperty.Register(
            "DrawableElement", typeof(DrawableElement), typeof(SelectScaleAttach),
            new PropertyMetadata(default(DrawableElement)));

        public DrawableElement DrawableElement
        {
            get { return (DrawableElement) GetValue(DrawableElementProperty); }
            set { SetValue(DrawableElementProperty, value); }
        }

        public SelectScaleAttach()
        {
            DependencyPropertyDescriptor
                .FromProperty(SelectScaleAttach.DrawableElementProperty, typeof(SelectScaleAttach))
                .AddValueChanged(this, Handler);
        }

        private DrawableElement _drawElement;

        private void Handler(object sender, EventArgs e)
        {
            _drawElement = this.DrawableElement;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseRightButtonDown += AssociatedObject_MouseRightButtonDown;
            this.AssociatedObject.MouseRightButtonUp += AssociatedObject_MouseRightButtonUp;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        }

        private bool _canDraw;

        private Point _startPoint;

        private void AssociatedObject_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_canDraw)
            {
                _canDraw = false;
                _drawElement.Clear();
                var position = e.GetPosition(AssociatedObject);
                if ((position - _startPoint).Length <= MinPointDistance)
                {
                    return;
                }

                var drawingElementRenderSize = AssociatedObject.RenderSize;
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

                OnScaled(new SelectionArgs()
                {
                    FullRect = new Rect(drawingElementRenderSize),
                    SelectRect = new Rect(_startPoint, position),
                });
            }
        }

        private void AssociatedObject_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(AssociatedObject);
            _canDraw = _drawElement != null && _startPoint.X > 0 && _startPoint.Y > 0;
        }


        private readonly Pen _drawPen = new Pen(Brushes.Black, 0.5) {DashStyle = DashStyles.Dot};

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (_canDraw)
            {
                var position = e.GetPosition(AssociatedObject);
                _drawElement.RenderVisual((context =>
                {
                    context.DrawRectangle(null, _drawPen, new Rect(_startPoint, position));
                }));
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.MouseRightButtonDown -= AssociatedObject_MouseRightButtonDown;
            this.AssociatedObject.MouseRightButtonUp -= AssociatedObject_MouseRightButtonUp;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        }

        protected virtual void OnScaled(SelectionArgs e)
        {
            Scaled?.Invoke(this, e);
        }
    }
}