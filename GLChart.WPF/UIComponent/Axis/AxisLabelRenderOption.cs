using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace GLChart.WPF.UIComponent.Axis
{
    /// <summary>
    /// 坐标轴渲染参数
    /// </summary>
    public class AxisLabelRenderOption
    {
        public double FontEmSize { get; set; } = 16;

        public Brush Foreground { get; set; } = Brushes.Black;

        public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentUICulture;

        public Typeface Typeface { get; set; } = new Typeface(SystemFonts.CaptionFontFamily,
            FontStyles.Normal,
            FontWeights.Normal,
            FontStretches.Normal);

        public AxisLabelRenderOption()
        {
            _textHeightLazy = new Lazy<double>((() =>
            {
                return new FormattedText(TestString, CultureInfo, FlowDirection.LeftToRight, Typeface, FontEmSize,
                    Foreground, 1).Height;
            }));
        }

        private readonly Lazy<double> _textHeightLazy;

        public const string TestString = "X";

        public double TextHeight
        {
            get => _textHeightLazy.Value;
        }
    }
}