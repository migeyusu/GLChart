using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.UIComponent.Axis;

namespace GLChart.WPF.UIComponent.Control
{
    /// <summary>
    /// 绘制坐标轴的图层
    /// </summary>
    public abstract class AxisElement : FrameworkElement
    {
        static AxisElement()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AxisElement),
                new FrameworkPropertyMetadata(typeof(AxisElement)));
        }

        public abstract AxisOption Option { get; set; }

        protected AxisElement()
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var labelGenerationOption = this.Option;
            if (labelGenerationOption == null)
            {
                return;
            }

            var scrollRange = this.Option.CurrentViewRange;
            if (scrollRange.IsEmpty())
            {
                return;
            }

            RenderAxis(labelGenerationOption, drawingContext);
        }

        /// <summary>
        /// 渲染坐标轴
        /// </summary>
        /// <param name="labelGenerationOption"></param>
        /// <param name="context"></param>
        protected abstract void RenderAxis(AxisOption labelGenerationOption, DrawingContext context);
    }
}