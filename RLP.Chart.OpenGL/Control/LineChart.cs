using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using OpenTK.Graphics;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Configuration;
using OpenTkWPFHost.Control;
using OpenTkWPFHost.Core;
using RLP.Chart.Interface;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.CollisionDetection;
using RLP.Chart.OpenGL.Interaction;
using RLP.Chart.OpenGL.Renderer;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;

namespace RLP.Chart.OpenGL.Control
{
    /// <summary>
    /// 基于2d xy坐标系的图
    /// </summary>
    [TemplatePart(Name = ThreadOpenTkControl, Type = typeof(Coordinate2D))]
    [TemplatePart(Name = ThreadOpenTkControl, Type = typeof(BitmapOpenTkControl))]
    [TemplatePart(Name = SelectScaleElement, Type = typeof(MouseSelect))]
//    [TemplatePart(Name = Popup, Type = typeof(ToolTip))]
    public class LineChart : System.Windows.Controls.Control, ISeriesChart<LineRenderer>
    {
        public const string ThreadOpenTkControl = "ThreadOpenTkControl";


        public const string CoordinateElementName = "Coordinate";

        public const string SelectScaleElement = "SelectScaleElement";

        static LineChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineChart),
                new FrameworkPropertyMetadata(typeof(LineChart)));
        }

        public static readonly DependencyProperty ToolTipTemplateProperty = DependencyProperty.Register(
            "ToolTipTemplate", typeof(DataTemplate), typeof(LineChart),
            new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ToolTipTemplate
        {
            get { return (DataTemplate)GetValue(ToolTipTemplateProperty); }
            set { SetValue(ToolTipTemplateProperty, value); }
        }

        #region collision

        public CollisionEnum CollisionEnum { get; set; } = CollisionEnum.SpacialHash;

        /// <summary>
        /// “碰撞种子” ,影响碰撞检测的性能
        /// </summary>
        public Boundary2D CollisionSeed { get; set; } = new Boundary2D(0, 100, 0, 100);

        /// <summary>
        /// 初始化碰撞检测的边界，减少碰撞网格的分配开销
        /// </summary>
        public Boundary2D InitialCollisionGridBoundary { get; set; } = new Boundary2D(0, 100, 0, 100);

        #endregion

        #region coordinate

        public static readonly DependencyProperty XLabelGenerationOptionProperty = DependencyProperty.Register(
            "XLabelGenerationOption", typeof(LabelGenerationOption), typeof(LineChart),
            new PropertyMetadata(default(LabelGenerationOption)));

        public LabelGenerationOption XLabelGenerationOption
        {
            get { return (LabelGenerationOption)GetValue(XLabelGenerationOptionProperty); }
            set { SetValue(XLabelGenerationOptionProperty, value); }
        }

        public static readonly DependencyProperty YLabelGenerationOptionProperty = DependencyProperty.Register(
            "YLabelGenerationOption", typeof(LabelGenerationOption), typeof(LineChart),
            new PropertyMetadata(default(LabelGenerationOption)));

        public LabelGenerationOption YLabelGenerationOption
        {
            get { return (LabelGenerationOption)GetValue(YLabelGenerationOptionProperty); }
            set { SetValue(YLabelGenerationOptionProperty, value); }
        }

        //ActualRegion : SettingRegion
        public virtual Region2D DisplayRegion
        {
            get { return CoordinateRenderer.AutoYAxisEnable ? ActualRegion : SettingRegion; }
            set { SetValue(SettingRegionProperty, value); }
        }

        public static readonly DependencyProperty SettingRegionProperty = DependencyProperty.Register(
            "SettingRegion", typeof(Region2D), typeof(LineChart),
            new PropertyMetadata(default(Region2D)));

        /// <summary>
        /// 设置的视域
        /// </summary>
        public Region2D SettingRegion
        {
            get { return (Region2D)GetValue(SettingRegionProperty); }
            set { SetValue(SettingRegionProperty, value); }
        }

        public static readonly DependencyProperty ActualRegionProperty = DependencyProperty.Register(
            "ActualRegion", typeof(Region2D), typeof(LineChart),
            new PropertyMetadata(default(Region2D)));

        /// <summary>
        /// 当前实际视域
        /// </summary>
        public Region2D ActualRegion
        {
            get { return (Region2D)GetValue(ActualRegionProperty); }
            set { SetValue(ActualRegionProperty, value); }
        }

        /// <summary>
        /// x轴缩放边界，超过该边界将重置
        /// </summary>
        public ScrollRange AxisXScrollBoundary { get; set; }

        /// <summary>
        /// X轴最小视野宽度
        /// </summary>
        public float MinAxisXViewSize { get; set; }

        /// <summary>
        /// Y轴最小视野宽度
        /// </summary>
        public float MinAxisYViewSize { get; set; }

        /// <summary>
        /// 自适应Y轴的默认区间，当界面内没有元素时显示该区间
        /// </summary>
        public ScrollRange DefaultAutoSizeAxisYRange { get; set; }

        public static readonly DependencyProperty AutoYAxisProperty = DependencyProperty.Register(
            "AutoYAxis", typeof(bool), typeof(LineChart), new PropertyMetadata(true));

        public virtual bool AutoYAxisEnable
        {
            get { return (bool)GetValue(AutoYAxisProperty); }
            set { SetValue(AutoYAxisProperty, value); }
        }

        #endregion

        #region render

        private GLSettings _glSettings = new GLSettings()
        {
            GraphicsContextFlags = ContextFlags.Offscreen,
        };

        /// <summary>
        /// gl 设置
        /// </summary>
        public GLSettings GlSettings
        {
            get => _glSettings;
            set
            {
                _glSettings = value;
                if (OpenTkControl != null)
                {
                    OpenTkControl.GlSettings = value;
                }
            }
        }


        public static readonly DependencyProperty IsShowFpsProperty = DependencyProperty.Register(
            nameof(IsShowFps), typeof(bool), typeof(LineChart), new PropertyMetadata(default(bool)));

        public bool IsShowFps
        {
            get { return (bool)GetValue(IsShowFpsProperty); }
            set { SetValue(IsShowFpsProperty, value); }
        }

        public static Dispatcher AppDispatcher => DispatcherLazy.Value;

        private static readonly Lazy<Dispatcher> DispatcherLazy = new Lazy<Dispatcher>(
            () => Application.Current.Dispatcher,
            LazyThreadSafetyMode.ExecutionAndPublication);

        #endregion


        private MouseSelect _scaleElement;

        private readonly CollisionGrid _nodeGrid = new CollisionGrid();

        private ToolTip _toolTip;

        private Node _popupNode = default;

        public LineChart() : base()
        {
            CoordinateRenderer = new AutoHeight2DRenderer(new BaseRenderer[] { LineSeriesRenderer });
            DependencyPropertyDescriptor.FromProperty(SettingRegionProperty, typeof(LineChart))
                .AddValueChanged(this, SettingRegionChangedHandler);
            DependencyPropertyDescriptor.FromProperty(AutoYAxisProperty, typeof(LineChart))
                .AddValueChanged(this, AutoYAxisChanged_Handler);
            CoordinateRenderer.AutoYAxisEnable = (bool)AutoYAxisProperty.DefaultMetadata.DefaultValue;
        }

        protected BitmapOpenTkControl OpenTkControl;

        protected AutoHeight2DRenderer CoordinateRenderer;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            OpenTkControl = GetTemplateChild(ThreadOpenTkControl) as BitmapOpenTkControl;
            OpenTkControl.GlSettings = this.GlSettings;
            CoordinateRenderer.ActualRegionChanged += SeriesRendererHost_AutoAxisYCompleted;
            OpenTkControl.Renderer = CoordinateRenderer;
            OpenTkControl.RenderErrorReceived += OpenTkControlOnRenderErrorReceived;
            OpenTkControl.OpenGlErrorReceived += OpenTkControlOnOpenGlErrorReceived;
            _scaleElement = GetTemplateChild(SelectScaleElement) as MouseSelect;
            _scaleElement.Selected += scaleElement_Scaled;
            _scaleElement.MouseMove += _openTkControl_MouseMove;
            _scaleElement.MouseLeave += _scaleElement_MouseLeave;
            _scaleElement.MouseLeftButtonDown += _scaleElement_MouseLeftButtonDown;
            _scaleElement.MouseLeftButtonUp += _scaleElement_MouseLeftButtonUp;
            _toolTip = new ToolTip()
            {
                IsOpen = false,
                StaysOpen = true,
                PlacementTarget = OpenTkControl,
                Placement = PlacementMode.Relative,
                ContentTemplate = this.ToolTipTemplate,
            };
        }

        #region render event handler

        /*当需要同步依赖属性和独立变量时，使用观察者，倾向于在load或apply template后再绑定以防止未加载的空控件，同时必须初始化值*/

        protected virtual void SettingRegionChangedHandler(object sender, EventArgs e)
        {
            var region = this.SettingRegion;
            this.CoordinateRenderer.TargetRegion = region;
            this.ActualRegion = region; //直接拷贝数据
            // Debug.WriteLine(region.ToString());
        }

        private void AutoYAxisChanged_Handler(object sender, EventArgs e)
        {
            CoordinateRenderer.AutoYAxisEnable = this.AutoYAxisEnable;
        }

        protected virtual void SeriesRendererHost_AutoAxisYCompleted(Region2D region)
        {
            // AppDispatcher.Invoke(() => { OpenTkControl.IsRenderContinuously = false; });
            AppDispatcher.InvokeAsync(() => { this.ActualRegion = region; });
        }


        private static void OpenTkControlOnOpenGlErrorReceived(object sender, OpenGlErrorArgs e)
        {
            Trace.WriteLine(e.ToString());
        }

        private static void OpenTkControlOnRenderErrorReceived(object sender, RenderErrorArgs e)
        {
            Trace.WriteLine($"{e.Exception.Message}");
        }

        #endregion

        public virtual void AttachWindow(Window hostWindow)
        {
            this.OpenTkControl.Start(hostWindow);
        }

        public virtual void DetachWindow()
        {
            this.OpenTkControl.Close();
        }

        #region interaction

        private Point _startMovePoint;

        private bool _isMouseLeftButtonPressed;

        private Region2D _startMoveRegion;

        private void _scaleElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseLeftButtonPressed = false;
        }

        private void _scaleElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseLeftButtonPressed = true;
            _startMovePoint = e.GetPosition(OpenTkControl);
            _startMoveRegion = this.DisplayRegion;
        }

        private void _scaleElement_MouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseLeftButtonPressed = false;
            if (_toolTip.IsOpen)
            {
                _toolTip.IsOpen = false;
            }
        }

        private void _openTkControl_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(OpenTkControl); //像素
            var coordinateRegion = this.DisplayRegion;
            var xRange = coordinateRegion.XRange;
            var yRange = coordinateRegion.YRange;
            var windowsRectToGlMapping =
                new WindowsToGlRecMapping(xRange, yRange, new Rect(OpenTkControl.RenderSize));
            if (_isMouseLeftButtonPressed)
            {
                var xPixel = _startMovePoint.X - position.X;
                var xOffset = windowsRectToGlMapping.GetXOffset(xPixel);
                double yOffset = 0;
                if (!CoordinateRenderer.AutoYAxisEnable)
                {
                    var yPixel = _startMovePoint.Y - position.Y;
                    yOffset = windowsRectToGlMapping.GetYOffset(-yPixel);
                }

                var region = _startMoveRegion.CreateOffset(xOffset, yOffset);
                if (region.XRange.End > AxisXScrollBoundary.End)
                {
                    region.ChangeXRange(xRange);
                }
                else if (region.XRange.Start < AxisXScrollBoundary.Start)
                {
                    region.ChangeXRange(xRange);
                }

                this.DisplayRegion = region;
                OnChangeRegionRequest(region);
            }
            else
            {
                var mapGlPoint = windowsRectToGlMapping.MapGlPoint(position);
                var xDistance = windowsRectToGlMapping.XScaleRatio * 10;
                var yDistance = windowsRectToGlMapping.YScaleRatio * 10;
                var ellipseGeometry = new Ellipse(mapGlPoint, (float)xDistance, (float)yDistance);
                if (_nodeGrid.TrySearch(ellipseGeometry, out var point, out var layer))
                {
                    if (!_popupNode.Equals(point))
                    {
                        _popupNode = point;
                        var mapWinPoint = windowsRectToGlMapping.MapWinPoint(point.Point);
                        var seriesItem = _items.First((item => item.CollisionGridLayer.Equals(layer)));
                        _toolTip.HorizontalOffset = mapWinPoint.X + 10;
                        _toolTip.VerticalOffset = mapWinPoint.Y + 10;
                        _toolTip.Content =
                            new MouseHoverNodeData(seriesItem.LineColor, point.Data,
                                seriesItem.Title);
                    }

                    if (!_toolTip.IsOpen)
                    {
                        _toolTip.IsOpen = true;
                    }

                    return;
                }
            }

            if (_toolTip.IsOpen)
            {
                _toolTip.IsOpen = false;
            }
        }


        public event Action<Region2D> ChangeRegionRequest;

        protected virtual void OnChangeRegionRequest(Region2D obj)
        {
            ChangeRegionRequest?.Invoke(obj);
        }

        private void scaleElement_Scaled(object sender, SelectionArgs e)
        {
            var oldRegion = this.DisplayRegion;
            var scale = new WindowsToGlRecMapping(oldRegion.XRange, oldRegion.YRange, e.FullRect);
            scale.ScaleByRect(e.SelectRect, out var xRange, out var yRange);
            var newRegion = new Region2D(xRange, yRange);
            if (this.CoordinateRenderer.AutoYAxisEnable)
            {
                this.AutoYAxisEnable = false;
            }

            if (newRegion.Height < MinAxisYViewSize)
            {
                newRegion.ChangeYRange(oldRegion.YRange);
            }

            if (newRegion.Width < MinAxisXViewSize)
            {
                newRegion.ChangeXRange(oldRegion.XRange);
            }

            if (!newRegion.Equals(oldRegion))
            {
                this.DisplayRegion = newRegion;
                OnChangeRegionRequest(newRegion);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            var originDisplayRegion = this.DisplayRegion;
            var xRange = originDisplayRegion.XRange;
            var newRange = e.Scale(xRange);
            if (newRange.Range < this.MinAxisXViewSize)
            {
                return;
            }

            if (newRange.Start < this.AxisXScrollBoundary.Start)
            {
                newRange = newRange.WithStart(this.AxisXScrollBoundary.Start);
            }

            if (newRange.End > this.AxisXScrollBoundary.End)
            {
                newRange = newRange.WithEnd(this.AxisXScrollBoundary.End);
            }

            if (!newRange.Equals(xRange))
            {
                var finalRange = originDisplayRegion.ChangeXRange(newRange);
                this.DisplayRegion = finalRange;
                this.OnChangeRegionRequest(finalRange);
            }
        }

        #endregion

        #region collection

        protected LineSeriesRenderer LineSeriesRenderer =
            new LineSeriesRenderer(new Shader("Shaders/LineShader/shader.vert",
                "Shaders/LineShader/shader.frag"));

        private readonly List<LineRenderer> _items = new List<LineRenderer>(5);

        public IReadOnlyList<LineRenderer> SeriesItems =>
            new ReadOnlyCollection<LineRenderer>(_items);

        public LineRenderer NewSeries()
        {
            var collisionSeed = this.CollisionSeed;
            switch (CollisionEnum)
            {
                case CollisionEnum.SpacialHash:
                    return new LineRenderer(new SpacialHashSet(collisionSeed.XSpan,
                        SpacialHashSet.Algorithm.XMapping,
                        (int)InitialCollisionGridBoundary.XSpan));
                case CollisionEnum.UniformGrid:
                    var collisionGridLayer = new Point2DCollisionGridLayer(InitialCollisionGridBoundary,
                        collisionSeed.XSpan, collisionSeed.YSpan, new LinkedListGridCellFactory());
                    return new LineRenderer(collisionGridLayer);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Add(LineRenderer item)
        {
            if (item is LineRenderer seriesItem)
            {
                this._nodeGrid.AddLayer(seriesItem.CollisionGridLayer);
                this.LineSeriesRenderer.Add(seriesItem);
                this._items.Add(seriesItem);
            }
        }

        public void Remove(LineRenderer item)
        {
            if (item is LineRenderer lineSeries)
            {
                this._items.Remove(lineSeries);
                this.LineSeriesRenderer.Remove(lineSeries);
                this._nodeGrid.Remove(lineSeries.CollisionGridLayer);
            }
        }

        public void Clear()
        {
            this.LineSeriesRenderer.Clear();
            this._items.Clear();
            this._nodeGrid.Clear();
        }

        #endregion
    }
}