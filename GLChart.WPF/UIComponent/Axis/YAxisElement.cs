using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.UIComponent.Control;

namespace GLChart.WPF.UIComponent.Axis
{
    public class YAxisElement : AxisElement
    {
        public static readonly DependencyProperty OptionProperty = DependencyProperty.Register(
            "Option", typeof(AxisOption), typeof(AxisElement),
            new FrameworkPropertyMetadata(new AxisYOption(),
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
            var height = this.RenderSize.Height;
            var labels =
                labelGenerationOption.GenerateLabels(height, height);
            var fontSize = option.FontEmSize;
            var typeface = option.Typeface;
            var cultureInfo = option.CultureInfo;
            var foreground = option.Foreground;
            double maxWidth = 0;
            var direction = FlowDirection.LeftToRight;
            foreach (var label in labels)
            {
                var text = new FormattedText(label.Text, cultureInfo, direction, typeface, fontSize,
                    foreground, 1);
                var textHeight = text.Height;
                var textWidth = text.Width;
                if (maxWidth < textWidth)
                {
                    maxWidth = textWidth;
                }

                context.DrawText(text
                    , new Point(0, label.Location - textHeight / 2));
            }

            if (!maxWidth.AlmostSame(this.Width))
            {
                this.Width = maxWidth;
            }
        }
    }
}