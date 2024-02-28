using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render;
using GLChart.WPF.Render.CollisionDetection;
using GLChart.WPF.Render.Renderer;
using GLChart.WPF.UIComponent.Axis;
using GLChart.WPF.UIComponent.Interaction;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Configuration;
using OpenTkWPFHost.Control;
using OpenTkWPFHost.Core;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;

namespace GLChart.WPF.UIComponent.Control
{
    /// <summary>
    /// 基于2d xy坐标系的图
    /// </summary>
    [TemplatePart(Name = ThreadOpenTkControl, Type = typeof(Coordinate2D))]
    [TemplatePart(Name = ThreadOpenTkControl, Type = typeof(BitmapOpenTkControl))]
    [TemplatePart(Name = SelectScaleElement, Type = typeof(MouseSelect))]
    public class LineChart : System.Windows.Controls.Control, ISeriesChart<ILine2D>
    {
        private const string ThreadOpenTkControl = "ThreadOpenTkControl";

        private const string CoordinateElementName = "Coordinate";

        private const string SelectScaleElement = "SelectScaleElement";

        private const string ToolTipName = "ToolTip";

        static LineChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineChart),
                new FrameworkPropertyMetadata(typeof(LineChart)));
        }

        public static readonly DependencyProperty ToolTipTemplateProperty = DependencyProperty.Register(
            nameof(ToolTipTemplate), typeof(DataTemplate), typeof(LineChart),
            new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ToolTipTemplate
        {
            get => (DataTemplate)GetValue(ToolTipTemplateProperty);
            set => SetValue(ToolTipTemplateProperty, value);
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

        public static readonly DependencyProperty AxisXOptionProperty = DependencyProperty.Register(
            nameof(AxisXOption), typeof(AxisOption), typeof(LineChart),
            new PropertyMetadata(new AxisOption()));

        public AxisOption AxisXOption
        {
            get => (AxisOption)GetValue(AxisXOptionProperty);
            set => SetValue(AxisXOptionProperty, value);
        }

        public static readonly DependencyProperty AxisYOptionProperty = DependencyProperty.Register(
            nameof(AxisYOption), typeof(AxisOption), typeof(LineChart),
            new PropertyMetadata(new AxisOption()));

        public AxisOption AxisYOption
        {
            get => (AxisOption)GetValue(AxisYOptionProperty);
            set => SetValue(AxisYOptionProperty, value);
        }

        public static readonly DependencyProperty SettingRegionProperty = DependencyProperty.Register(
            nameof(SettingRegion), typeof(Region2D), typeof(LineChart),
            new PropertyMetadata(default(Region2D), SettingRegionChangedCallback));

        /// <summary>
        /// 设置的视域
        /// </summary>
        public Region2D SettingRegion
        {
            get => (Region2D)GetValue(SettingRegionProperty);
            set => SetValue(SettingRegionProperty, value);
        }

        public static readonly DependencyProperty ActualRegionProperty = DependencyProperty.Register(
            nameof(ActualRegion), typeof(Region2D), typeof(LineChart),
            new PropertyMetadata(default(Region2D)));

        /// <summary>
        /// 当前实际视域
        /// </summary>
        public Region2D ActualRegion
        {
            get => (Region2D)GetValue(ActualRegionProperty);
            set => SetValue(ActualRegionProperty, value);
        }

        public static readonly DependencyProperty DefaultYRangeProperty = DependencyProperty.Register(
            nameof(DefaultYRange), typeof(ScrollRange), typeof(LineChart),
            new PropertyMetadata(new ScrollRange(0, 100), (DefaultYRangeChangedCallback)));

        /// <summary>
        /// 自适应Y轴的默认区间，当界面内没有元素时显示该区间
        /// </summary>
        public ScrollRange DefaultYRange
        {
            get => (ScrollRange)GetValue(DefaultYRangeProperty);
            set => SetValue(DefaultYRangeProperty, value);
        }

        public static readonly DependencyProperty IsAutoYAxisEnableProperty = DependencyProperty.Register(
            nameof(IsAutoYAxisEnable), typeof(bool), typeof(LineChart),
            new PropertyMetadata(true, AutoYAxisChangedCallback));

        public virtual bool IsAutoYAxisEnable
        {
            get => (bool)GetValue(IsAutoYAxisEnableProperty);
            set => SetValue(IsAutoYAxisEnableProperty, value);
        }

        #endregion

        #region render

        // ReSharper disable once InconsistentNaming
        public static readonly DependencyProperty GLSettingsProperty = DependencyProperty.Register(
            nameof(GLSettings), typeof(GLSettings), typeof(LineChart), new PropertyMetadata(new GLSettings()
            {
                GraphicsContextFlags = ContextFlags.Offscreen,
            }));

        // ReSharper disable once InconsistentNaming
        public GLSettings GLSettings
        {
            get => (GLSettings)GetValue(GLSettingsProperty);
            set => SetValue(GLSettingsProperty, value);
        }

        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
            nameof(BackgroundColor), typeof(Color), typeof(LineChart),
            new PropertyMetadata(Colors.White, BackgroundColorChangedCallback));

        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public static readonly DependencyProperty IsShowFpsProperty = DependencyProperty.Register(
            nameof(IsShowFps), typeof(bool), typeof(LineChart), new PropertyMetadata(default(bool)));

        public bool IsShowFps
        {
            get => (bool)GetValue(IsShowFpsProperty);
            set => SetValue(IsShowFpsProperty, value);
        }

        private readonly Coordinate2DRenderer _coordinateRenderer;

        #endregion

        private MouseSelect _scaleElement;

        private Node _popupNode;

        public LineChart()
        {
            var color = (Color)BackgroundColorProperty.DefaultMetadata.DefaultValue;
            _coordinateRenderer = new Coordinate2DRenderer(new BaseRenderer[] { LineSeriesRenderer })
            {
                BackgroundColor = new Color4(color.A, color.R, color.G, color.B),
                AutoYAxisWorking = (bool)IsAutoYAxisEnableProperty.DefaultMetadata.DefaultValue,
                DefaultAxisYRange = (ScrollRange)DefaultYRangeProperty.DefaultMetadata.DefaultValue,
                TargetRegion = (Region2D)SettingRegionProperty.DefaultMetadata.DefaultValue
            };
        }

        protected BitmapOpenTkControl OpenTkControl;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            OpenTkControl = GetTemplateChild(ThreadOpenTkControl) as BitmapOpenTkControl;
            _coordinateRenderer.ActualRegionChanged += OnRendererHostAutoAxisYCompleted;
            Debug.Assert(OpenTkControl != null, nameof(OpenTkControl) + " != null");
            OpenTkControl.Renderer = _coordinateRenderer;
            OpenTkControl.RenderErrorReceived += OpenTkControlOnRenderErrorReceived;
            OpenTkControl.OpenGlErrorReceived += OpenTkControlOnOpenGlErrorReceived;
            _scaleElement = GetTemplateChild(SelectScaleElement) as MouseSelect;
            Debug.Assert(_scaleElement != null, nameof(_scaleElement) + " != null");
            _scaleElement.Selected += scaleElement_Scaled;
            _toolTip = new ToolTip()
            {
                IsOpen = false,
                PlacementTarget = OpenTkControl,
                Placement = PlacementMode.Relative,
                ContentTemplate = ToolTipTemplate
            };
        }

        #region render event handler

        private static void DefaultYRangeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LineChart lineChart)
            {
                lineChart._coordinateRenderer.DefaultAxisYRange = (ScrollRange)e.NewValue;
            }
        }

        private static void BackgroundColorChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is LineChart lineChart)
            {
                var color = (Color)e.NewValue;
                lineChart._coordinateRenderer.BackgroundColor = new Color4(color.A, color.R, color.G, color.B);
            }
        }

        private static void SettingRegionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LineChart lineChart)
            {
                var region = (Region2D)e.NewValue;
                lineChart._coordinateRenderer.TargetRegion = region;
                lineChart.ActualRegion = region;
            }
        }

        private static void AutoYAxisChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LineChart lineChart)
            {
                lineChart._coordinateRenderer.AutoYAxisWorking = (bool)e.NewValue;
            }
        }

        protected virtual void OnRendererHostAutoAxisYCompleted(Region2D region)
        {
            this.Dispatcher.InvokeAsync(() => { this.ActualRegion = region; });
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

        private Region2D _startMoveRegion;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _startMovePoint = e.GetPosition(OpenTkControl);
            _startMoveRegion = this.ActualRegion;
        }

        private ToolTip _toolTip;

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (_toolTip.IsOpen)
            {
                _toolTip.IsOpen = false;
            }
        }

        private readonly CollisionGridPoint2D _nodeGrid = new CollisionGridPoint2D();

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var position = e.GetPosition(OpenTkControl); //像素
            var coordinateRegion = this.ActualRegion;
            var winToGlMapping =
                new WindowsGlCoordinateMapping(coordinateRegion, new Rect(OpenTkControl.RenderSize));
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this._coordinateRenderer.AutoYAxisWorking)
                {
                    this.IsAutoYAxisEnable = false;
                }

                var xPixel = _startMovePoint.X - position.X;
                var xOffset = winToGlMapping.GetXOffset(xPixel);
                var yPixel = _startMovePoint.Y - position.Y;
                var yOffset = winToGlMapping.GetYOffset(-yPixel);
                var newRegion = _startMoveRegion.OffsetNew(xOffset, yOffset);
                var xZoomBoundary = AxisXOption.ZoomBoundary;
                var right = xZoomBoundary.End;
                var start = xZoomBoundary.Start;
                var xExtend = newRegion.XExtend;
                if (newRegion.Right > right)
                {
                    var xStart = right - xExtend;
                    newRegion.SetLeft(xStart);
                    newRegion.SetRight(right);
                }
                else if (newRegion.Left < start)
                {
                    var xEnd = start + xExtend;
                    newRegion.SetLeft(start);
                    newRegion.SetRight(xEnd);
                }

                var yZoomBoundary = AxisYOption.ZoomBoundary;
                var end = yZoomBoundary.End;
                var bottom = yZoomBoundary.Start;
                var yExtend = newRegion.YExtend;
                if (newRegion.Top > end)
                {
                    var yStart = end - yExtend;
                    newRegion.SetTop(end);
                    newRegion.SetBottom(yStart);
                }
                else if (newRegion.Bottom < bottom)
                {
                    var yEnd = bottom + yExtend;
                    newRegion.SetBottom(bottom);
                    newRegion.SetTop(yEnd);
                }

                this.SettingRegion = newRegion;
            }
            else
            {
                var mapGlPoint = winToGlMapping.GetGlPointByWindowsPoint(position);
                var xDistance = winToGlMapping.XScaleRatio * 10;
                var yDistance = winToGlMapping.YScaleRatio * 10;
                var ellipse = new Ellipse(mapGlPoint, (float)xDistance, (float)yDistance);
                if (_nodeGrid.TrySearch(ellipse, out var point, out var layer))
                {
                    if (!_popupNode.Equals(point))
                    {
                        _popupNode = point;
                        var winPoint = winToGlMapping.GetWindowsPointByGlPoint(point.Point);
                        var seriesItem = _items.First((item => item.CollisionLayer.Equals(layer)));
                        _toolTip.HorizontalOffset = winPoint.X + 10;
                        _toolTip.VerticalOffset = winPoint.Y + 10;
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

        private void scaleElement_Scaled(object sender, SelectionArgs e)
        {
            var oldRegion = this.ActualRegion;
            var scale = new WindowsGlCoordinateMapping(oldRegion, e.FullRect);
            scale.ScaleByRect(e.SelectRect, out var xRange, out var yRange);
            var newRegion = new Region2D(xRange, yRange);
            if (this._coordinateRenderer.AutoYAxisWorking)
            {
                this.IsAutoYAxisEnable = false;
            }

            if (newRegion.Height < AxisYOption.MinDisplayExtent)
            {
                newRegion.ChangeYRange(oldRegion.YRange);
            }

            if (newRegion.Width < AxisXOption.MinDisplayExtent)
            {
                newRegion.ChangeXRange(oldRegion.XRange);
            }

            if (!newRegion.Equals(oldRegion))
            {
                this.SettingRegion = newRegion;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            var axisXOption = AxisXOption;
            var region = this.ActualRegion;
            var newXRange = region.XRange;
            var newYRange = region.YRange;
            if (axisXOption.ZoomEnable)
            {
                newXRange = e.Scale(newXRange);
                if (newXRange.Range < axisXOption.MinDisplayExtent)
                {
                    return;
                }

                var zoomBoundary = axisXOption.ZoomBoundary;
                if (newXRange.Start < zoomBoundary.Start)
                {
                    newXRange = newXRange.WithStart(zoomBoundary.Start);
                }

                if (newXRange.End > zoomBoundary.End)
                {
                    newXRange = newXRange.WithEnd(zoomBoundary.End);
                }
            }

            var axisYOption = AxisYOption;
            if (axisYOption.ZoomEnable)
            {
                newYRange = e.Scale(newYRange);
                if (newYRange.Range < axisYOption.MinDisplayExtent)
                {
                    return;
                }

                var zoomBoundary = axisYOption.ZoomBoundary;
                if (newYRange.Start < zoomBoundary.Start)
                {
                    newYRange = newYRange.WithStart(zoomBoundary.Start);
                }

                if (newYRange.End > zoomBoundary.End)
                {
                    newYRange = newYRange.WithEnd(zoomBoundary.End);
                }
            }

            var newRegion = new Region2D(newXRange, newYRange);
            if (!newRegion.Equals(this.ActualRegion))
            {
                this.SettingRegion = newRegion;
            }
        }

        #endregion

        #region collection

        protected readonly Line2DSeriesRenderer LineSeriesRenderer =
            new Line2DSeriesRenderer(new Shader("Render/Shaders/LineShader/shader.vert",
                "Render/Shaders/LineShader/shader.frag"));

        private readonly List<Line2DRenderer> _items = new List<Line2DRenderer>(5);

        public IReadOnlyList<ILine2D> SeriesItems =>
            new ReadOnlyCollection<Line2DRenderer>(_items);

        public ILine2D NewSeries()
        {
            var collisionSeed = this.CollisionSeed;
            Line2DRenderer lineRenderer;
            switch (CollisionEnum)
            {
                case CollisionEnum.SpacialHash:
                    lineRenderer = new Line2DRenderer(new SpacialHashCollisionPoint2DLayer(collisionSeed.XSpan,
                        SpacialHashCollisionPoint2DLayer.Algorithm.XMapping,
                        (int)InitialCollisionGridBoundary.XSpan));
                    break;
                case CollisionEnum.UniformGrid:
                    var collisionGridLayer = new CollisionGridPoint2DLayer(InitialCollisionGridBoundary,
                        collisionSeed.XSpan, collisionSeed.YSpan, new LinkedListGridCellFactory());
                    lineRenderer = new Line2DRenderer(collisionGridLayer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this._nodeGrid.AddLayer(lineRenderer.CollisionLayer);
            this.LineSeriesRenderer.Add(lineRenderer);
            this._items.Add(lineRenderer);
            return lineRenderer;
        }

        public void Remove(ILine2D line)
        {
            if (line is Line2DRenderer lineRenderer)
            {
                this._items.Remove(lineRenderer);
                this.LineSeriesRenderer.Remove(lineRenderer);
                this._nodeGrid.Remove(lineRenderer.CollisionLayer);
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