using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.UIComponent.Control;
using Point = System.Windows.Point;

namespace GLChart.WPF.UIComponent.Axis
{
    public class XAxisElement : AxisElement
    {
        public static readonly DependencyProperty OptionProperty = DependencyProperty.Register(
            "Option", typeof(AxisOption), typeof(AxisElement),
            new FrameworkPropertyMetadata(new AxisXOption(),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public override AxisOption Option
        {
            get { return (AxisOption)GetValue(OptionProperty); }
            set { SetValue(OptionProperty, value); }
        }

        protected override void RenderAxis(AxisOption labelGenerationOption,
            DrawingContext context)
        {
            var option = labelGenerationOption.RenderOption;
            var labels =
                labelGenerationOption.GenerateLabels(0, this.RenderSize.Width);
            var emSize = option.FontEmSize;
            var typeface = option.Typeface;
            var foreground = option.Foreground;
            var cultureInfo = option.CultureInfo;
            var direction = FlowDirection.LeftToRight;
            foreach (var label in labels)
            {
                var text = new FormattedText(label.Text, cultureInfo, direction, typeface,
                    emSize,
                    foreground, 1);
                context.DrawText(
                    text, new Point(label.Location - text.Width / 2, 0));
            }

            var renderHeight = option.TextHeight;
            if (!renderHeight.AlmostSame(this.Height))
            {
                this.Height = renderHeight;
            }
        }
    }
}