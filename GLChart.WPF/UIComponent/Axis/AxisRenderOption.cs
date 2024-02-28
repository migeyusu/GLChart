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
    public class AxisRenderOption
    {
        public double FontEmSize { get; set; }

        public Brush Foreground { get; set; }

        public CultureInfo CultureInfo { get; set; }

        public Typeface Typeface { get; set; }

        public AxisRenderOption()
        {
            _textHeightLazy = new Lazy<double>((() =>
            {
                return new FormattedText(TestString, CultureInfo, FlowDirection.LeftToRight, Typeface, FontEmSize,
                    Foreground, 1).Height;
            }));
        }

        private readonly Lazy<double> _textHeightLazy;

        public const string TestString = "M";

        public double TextHeight
        {
            get => _textHeightLazy.Value;
        }

        public static AxisRenderOption Default()
        {
            var defaultFont = SystemFonts.CaptionFontFamily;
            return new AxisRenderOption()
            {
                FontEmSize = 16,
                Foreground = Brushes.Black,
                Typeface = new Typeface(defaultFont,
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal),
                CultureInfo = CultureInfo.CurrentUICulture,
            };
        }
    }
}