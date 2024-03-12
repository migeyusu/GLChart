using System;
using System.Linq;
using System.Windows;
using GLChart.WPF.Base;
using GLChart.WPF.Render;
using GLChart.WPF.Render.Renderer;
using GLChart.WPF.UIComponent.Axis;

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
            _line = LineChart.NewSeries<RingLine2DRenderer>();
            var axisYOption = new AxisYOption
            {
                IsAutoSize = true,
                ViewRange = new ScrollRange(0, 500),
                ZoomBoundary = new ScrollRange(-5000, 5000)
            };
            var axisXOption = new AxisXOption
            {
                ViewRange = new ScrollRange(0, 1000),
                ZoomBoundary = new ScrollRange(-5000, 5000)
            };
            LineChart.AxisXOption = axisXOption;
            LineChart.AxisYOption = axisYOption;
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