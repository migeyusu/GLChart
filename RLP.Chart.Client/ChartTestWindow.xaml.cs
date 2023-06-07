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
using RLP.Chart.Interface;
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
            LineChart.SettingRegion = new Region2D(new ScrollRange(0, 100), new ScrollRange(0, 100));
            var random = new Random();
            for (int i = 0; i < 100; i++)
            {
                lineRenderer.Add(new Point2D(i, 200 + i + random.Next(-100, 100)));
            }
        }
    }
}