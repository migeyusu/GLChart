using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render;
using OpenTK.Mathematics;

namespace GLChart.Samples
{
    /// <summary>
    /// ChartTestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChartTestWindow : Window
    {
        private Random _random = new Random();

        public ChartTestWindow()
        {
            InitializeComponent();
            var random = new Random();
            foreach (var index in Enumerable.Range(0, 10))
            {
                var line = HistoricalGlChart.NewLine(20000);
                line.Title = $"Series {index}";
                line.Color = RandomColor();
                // _line = LineChart.NewSeries<RingLine2DRenderer>();
                var array = Enumerable.Range(_index, 20000)
                    .Select((i => new Point2D(i, 200 + i + random.Next(-100, 100))))
                    .Cast<IPoint2D>()
                    .ToList();
                line.AddRange(array);
            }

            _index += 1200;
        }


        private Color RandomColor()
        {
            byte[] colors = new byte[3];
            _random.NextBytes(colors);
            var fromRgb = Color.FromRgb(colors[0],colors[1],colors[2]);
            return fromRgb;
        }

        private readonly int _index = 0;

        private void ChartTestWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            /*var random = new Random();
            var array = Enumerable.Range(index, 1)
                .Select((i => new Point2D(i, 200 + i + random.Next(-100, 100))))
                .Cast<IPoint2D>()
                .ToList();
            _line.AddRange(array);
            index++;*/
            /*LineChart.IsAutoYAxisEnable = true;*/
            /*LineChart.SettingRegion = new Region2D(new ScrollRange(-10, 100), new ScrollRange(0, 310));*/
        }
    }
}