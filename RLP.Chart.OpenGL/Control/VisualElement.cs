using System;
using System.Windows;
using System.Windows.Media;

namespace RLP.Chart.OpenGL.Control
{
    /// <summary>
    /// 视觉元素
    /// </summary>
    public abstract class VisualElement : FrameworkElement
    {
        protected DrawingGroup DrawingGroup = new DrawingGroup();

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawDrawing(DrawingGroup);
            base.OnRender(drawingContext);
        }

        public void RenderVisual(Action<DrawingContext> action)
        {
            using (var drawingContext = DrawingGroup.Open())
            {
                action.Invoke(drawingContext);
            }
        }

        public void Clear()
        {
            DrawingGroup.Children.Clear();
        }
    }
}