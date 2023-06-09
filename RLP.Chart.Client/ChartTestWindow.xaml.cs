using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Configuration;
using RLP.Chart.Interface;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.Client
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

        private void ChartTestWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var lineRenderer = LineChart.NewSeries();
            LineChart.SettingRegion = new Region2D(new ScrollRange(0, 110), new ScrollRange(0, 510));
            LineChart.AxisXOption.ZoomBoundary = new ScrollRange(-1000, 1000);
            LineChart.AxisYOption.ZoomBoundary = new ScrollRange(-1000, 1000);
            var random = new Random();
            var array = Enumerable.Range(0, 100)
                .Select((i => new Point2D(i, 200 + i + random.Next(-100, 100))))
                .Cast<IPoint2D>()
                .ToList();
            lineRenderer.AddRange(array);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            LineChart.IsAutoYAxisEnable = true;
            /*LineChart.SettingRegion = new Region2D(new ScrollRange(-10, 100), new ScrollRange(0, 310));*/
        }
    }
}