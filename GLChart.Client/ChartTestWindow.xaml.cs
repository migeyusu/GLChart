using System;
using System.Linq;
using System.Windows;
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
        }

        private ILine2D _line;

        private int index = 0;

        private void ChartTestWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _line = LineChart.NewSeries();
            LineChart.IsAutoYAxisEnable = false;
            LineChart.SettingRegion = new Region2D(new ScrollRange(0, 200), new ScrollRange(0, 510));
            LineChart.AxisXOption.ZoomBoundary = new ScrollRange(-1000, 1000);
            LineChart.AxisYOption.ZoomBoundary = new ScrollRange(-1000, 1000);
            var random = new Random();
            var array = Enumerable.Range(index, 1200)
                .Select((i => new Point2D(i, 200 + i + random.Next(-100, 100))))
                .Cast<IPoint2D>()
                .ToList();
            _line.AddRange(array);
            index += 100;
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