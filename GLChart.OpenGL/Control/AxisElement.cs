using System.Windows;
using System.Windows.Media;
using RLP.Chart.Interface;

namespace RLP.Chart.OpenGL.Control
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

        public static readonly DependencyProperty AutoSizeProperty = DependencyProperty.Register(
            "AutoSize", typeof(bool), typeof(AxisElement), new PropertyMetadata(true));

        /// <summary>
        /// 是否自适应大小
        /// </summary>
        public bool AutoSize
        {
            get { return (bool) GetValue(AutoSizeProperty); }
            set { SetValue(AutoSizeProperty, value); }
        }

        public static readonly DependencyProperty RangeProperty = DependencyProperty.Register(
            "Range", typeof(ScrollRange), typeof(AxisElement),
            new FrameworkPropertyMetadata(default(ScrollRange), FrameworkPropertyMetadataOptions.AffectsRender));

        public ScrollRange Range
        {
            get { return (ScrollRange) GetValue(RangeProperty); }
            set { SetValue(RangeProperty, value); }
        }

        public static readonly DependencyProperty LabelGenerationOptionProperty = DependencyProperty.Register(
            "LabelGenerationOption", typeof(AxisOption), typeof(AxisElement),
            new FrameworkPropertyMetadata(new AxisOption(),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public AxisOption LabelGenerationOption
        {
            get { return (AxisOption) GetValue(LabelGenerationOptionProperty); }
            set { SetValue(LabelGenerationOptionProperty, value); }
        }

        protected AxisElement()
        {
            
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var labelGenerationOption = this.LabelGenerationOption;
            if (labelGenerationOption == null)
            {
                return;
            }

            var scrollRange = this.Range;
            if (scrollRange.IsEmpty())
            {
                return;
            }

            RenderAxis(labelGenerationOption, scrollRange, drawingContext);
        }

        /// <summary>
        /// 渲染坐标轴
        /// </summary>
        /// <param name="labelGenerationOption"></param>
        /// <param name="range"></param>
        /// <param name="context"></param>
        protected abstract void RenderAxis(AxisOption labelGenerationOption, ScrollRange range,
            DrawingContext context);
    }
}