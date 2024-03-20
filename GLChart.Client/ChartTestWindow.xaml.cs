using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render;

namespace GLChart.Samples
{
    /// <summary>
    /// ChartTestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChartTestWindow : Window
    {
        public ChartTestWindow()
        {
            InitializeComponent();
            _line = HistoricalGlChart.NewLine(2000);
            _line.Title = "Test";
            _line.Color = Colors.Red;
            // _line = LineChart.NewSeries<RingLine2DRenderer>();
            var random = new Random();
            var array = Enumerable.Range(index, 1200)
                .Select((i => new Point2D(i, 200 + i + random.Next(-100, 100))))
                .Cast<IPoint2D>()
                .ToList();
            _line.AddRange(array);
            index += 1200;
        }

        private ILine2D _line;

        private int index = 0;

        private void ChartTestWindow_OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var random = new Random();
            var array = Enumerable.Range(index, 1)
                .Select((i => new Point2D(i, 200 + i + random.Next(-100, 100))))
                .Cast<IPoint2D>()
                .ToList();
            _line.AddRange(array);
            index++;
            /*LineChart.IsAutoYAxisEnable = true;*/
            /*LineChart.SettingRegion = new Region2D(new ScrollRange(-10, 100), new ScrollRange(0, 310));*/
        }
    }
}