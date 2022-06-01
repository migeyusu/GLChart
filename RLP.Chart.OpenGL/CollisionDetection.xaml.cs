using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RLP.Chart.OpenGL
{
    public class VisualContainer : FrameworkElement
    {
        private readonly Visual _visual;

        public VisualContainer(Visual visual)
        {
            this._visual = visual;
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            return _visual;
        }
    }


    /// <summary>
    /// CollisionDetection.xaml 的交互逻辑
    /// </summary>
    public partial class CollisionDetectionWindow : Window
    {
        // private NodeGrid _grid = null;

        public CollisionDetectionWindow()
        {
            InitializeComponent();
            this.Loaded += CollisionDetection_Loaded;
        }

        public const int PointsCountLimit = 20000;
        public const int Count = 10;

        private void CollisionDetection_Loaded(object sender, RoutedEventArgs e)
        {
            var seriesItem = Chart.NewSeries();
            seriesItem.Visible = true;
            var random = new Random();
            var foo = new Renderer.Point2D[Count];
            for (var j = 0; j < Count; j++)
            {
                foo[j] = new Renderer.Point2D(j, random.Next(0, 10000) * 0.1f);
            }

            seriesItem.AddGeometries(foo.Select(point => new DefaultPoint(point)).ToArray());
            Chart.Add(seriesItem);
            /*XAxis.Range = new ScrollRange(0, Count);
            YAxis.Range = new ScrollRange(0, 1000);*/
            this.Chart.AttachWindow(this);
        }

        private void CollisionDetection_OnLoaded(object sender, RoutedEventArgs e)
        {
            /*var black = Brushes.Black;
            var drawingVisual = new DrawingVisual();
            var random = new Random();
            _grid = new NodeGrid(new Boundary(0f, 800f, 0f, 800f), 100);
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                for (int i = 0; i < 100000; i++)
                {
                    var pointX =  random.NextDouble() * 800;
                    var pointY = random.NextDouble() * 800;
                    drawingContext.DrawEllipse(black, null,
                        new Point(pointX, pointY), 0.5,0.5);
                    _grid.AddNode(new NodeData(i, new OpenGLRender.Point((float) pointX, (float) pointY)));
                }
            }


            var visualBrush = new VisualBrush(drawingVisual);
            this.Canvas.Children.Add(new VisualContainer(visualBrush.Visual));*/
        }

        private void CollisionDetection_OnMouseMove(object sender, MouseEventArgs e)
        {
            /*var position = e.GetPosition(this.Canvas);
            Trace.WriteLine(position.ToString());*/
            // this.TextBlock.Text = $"{position.X},{position.Y}";
            return;
            /*if (_grid.TrySearch(new OpenGLRender.Point((float) position.X, (float) position.Y), 3f, out var data))
            {
                this.TextBlock.Text = $"{data.ID},{data.Point}\r\n {position.X},{position.Y}";
            }
            else
            {
                
            }*/
        }
    }
}