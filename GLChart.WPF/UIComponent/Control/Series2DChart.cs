using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render;
using GLChart.WPF.Render.CollisionDetection;
using GLChart.WPF.Render.Renderer;
using GLChart.WPF.UIComponent.Axis;
using GLChart.WPF.UIComponent.Interaction;
using OpenTK.Mathematics;
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
    [TemplatePart(Name = ToolTipName, Type = typeof(ToolTip))]
    [TemplatePart(Name = CoordinateElementName, Type = typeof(Coordinate2D))]
    // [TemplatePart(Name = ThreadOpenTkControl, Type = typeof(BitmapOpenTkControl))]
    [TemplatePart(Name = SelectScaleElement, Type = typeof(MouseSelect))]
    public class Series2DChart : System.Windows.Controls.Control, ISeries2DChart
    {
        // private const string ThreadOpenTkControl = "ThreadOpenTkControl";

        private const string CoordinateElementName = "Coordinate";

        private const string SelectScaleElement = "SelectScaleElement";

        private const string ToolTipName = "ToolTip";

        static Series2DChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Series2DChart),
                new FrameworkPropertyMetadata(typeof(Series2DChart)));
        }

        public static readonly DependencyProperty ToolTipTemplateProperty = DependencyProperty.Register(
            nameof(ToolTipTemplate), typeof(DataTemplate), typeof(Series2DChart),
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
            nameof(AxisXOption), typeof(AxisXOption), typeof(Series2DChart),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender,
                AxisXPropertyChangedCallback));

        private static void AxisXPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var series2DChart = d as Series2DChart;
            if (series2DChart == null)
            {
                return;
            }

            series2DChart.ChangeAxisXOption(e);
        }

        private void OnAxisXViewChangedHandler(object? sender, EventArgs e)
        {
            this._coordinateRenderer.TargetXRange = AxisXOption.ViewRange;
        }

        private void ChangeAxisXOption(DependencyPropertyChangedEventArgs e)
        {
            var pd = DependencyPropertyDescriptor.FromProperty(AxisOption.ViewRangeProperty,
                typeof(AxisOption));
            if (e.OldValue is AxisXOption oldValue)
            {
                pd.RemoveValueChanged(oldValue, OnAxisXViewChangedHandler);
            }

            if (e.NewValue is AxisXOption newValue)
            {
                pd.AddValueChanged(newValue, OnAxisXViewChangedHandler);
                this._coordinateRenderer.TargetXRange = newValue.ViewRange;
            }
        }

        public AxisXOption AxisXOption
        {
            get => (AxisXOption)GetValue(AxisXOptionProperty);
            set => SetValue(AxisXOptionProperty, value);
        }

        public static readonly DependencyProperty AxisYOptionProperty = DependencyProperty.Register(
            nameof(AxisYOption), typeof(AxisYOption), typeof(Series2DChart),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender,
                AxisYPropertyChangedCallback));

        private static void AxisYPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var series2DChart = d as Series2DChart;
            if (series2DChart == null)
            {
                return;
            }

            series2DChart.ChangeAxisYOption(e);
        }

        private void ChangeAxisYOption(DependencyPropertyChangedEventArgs e)
        {
            var pd = DependencyPropertyDescriptor.FromProperty(AxisOption.ViewRangeProperty,
                typeof(AxisOption));
            var descriptor =
                DependencyPropertyDescriptor.FromProperty(AxisYOption.IsAutoSizeProperty, typeof(AxisYOption));
            if (e.OldValue is AxisYOption oldValue)
            {
                pd.RemoveValueChanged(oldValue, OnAxisYViewChangedHandler);
                descriptor.RemoveValueChanged(oldValue, OnAxisYAutoSizeChangedHandler);
            }

            if (e.NewValue is AxisYOption newValue)
            {
                pd.AddValueChanged(newValue, OnAxisYViewChangedHandler);
                descriptor.AddValueChanged(newValue, OnAxisYAutoSizeChangedHandler);
                var targetRegion = this._coordinateRenderer.TargetRegion;
                this._coordinateRenderer.TargetYRange = newValue.ViewRange;
                this._coordinateRenderer.AutoYAxisEnable = newValue.IsAutoSize;
            }
        }

        private void OnAxisYAutoSizeChangedHandler(object? sender, EventArgs e)
        {
            this._coordinateRenderer.AutoYAxisEnable = this.AxisYOption.IsAutoSize;
        }

        private void OnAxisYViewChangedHandler(object? sender, EventArgs e)
        {
            this._coordinateRenderer.TargetYRange = AxisYOption.ViewRange;
        }

        public AxisYOption AxisYOption
        {
            get => (AxisYOption)GetValue(AxisYOptionProperty);
            set => SetValue(AxisYOptionProperty, value);
        }

        /// <summary>
        /// 当前实际视域
        /// </summary>
        public Region2D ActualRegion
        {
            get { return new Region2D(AxisXOption.ViewRange, AxisYOption.ActualViewRange); }
        }

        public static readonly DependencyProperty DefaultYRangeProperty = DependencyProperty.Register(
            nameof(DefaultYRange), typeof(ScrollRange), typeof(Series2DChart),
            new PropertyMetadata(new ScrollRange(0, 100), (DefaultYRangeChangedCallback)));

        //todo: remove
        /// <summary>
        /// 自适应Y轴的默认区间，当界面内没有元素时显示该区间
        /// </summary>
        public ScrollRange DefaultYRange
        {
            get => (ScrollRange)GetValue(DefaultYRangeProperty);
            set => SetValue(DefaultYRangeProperty, value);
        }

        #endregion

        #region render

        public static readonly DependencyProperty OpenTKControlProperty = DependencyProperty.Register(
            nameof(OpenTKControl), typeof(OpenTkControlBase), typeof(Series2DChart),
            new PropertyMetadata(default(OpenTkControlBase)));

        public OpenTkControlBase OpenTKControl
        {
            get { return (OpenTkControlBase)GetValue(OpenTKControlProperty); }
            private set { SetValue(OpenTKControlProperty, value); }
        }

        private OpenTkControlBase _renderControl;

        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
            nameof(BackgroundColor), typeof(Color), typeof(Series2DChart),
            new PropertyMetadata(Colors.White, BackgroundColorChangedCallback));

        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public static readonly DependencyProperty IsShowFpsProperty = DependencyProperty.Register(
            nameof(IsShowFps), typeof(bool), typeof(Series2DChart), new PropertyMetadata(default(bool)));

        public bool IsShowFps
        {
            get => (bool)GetValue(IsShowFpsProperty);
            set => SetValue(IsShowFpsProperty, value);
        }

        private readonly Coordinate2DRenderer _coordinateRenderer;

        #endregion

        private MouseSelect? _scaleElement;

        public Series2DChart()
        {
            var color = (Color)BackgroundColorProperty.DefaultMetadata.DefaultValue;
            var defaultRange = (ScrollRange)AxisOption.ViewRangeProperty.DefaultMetadata.DefaultValue;
            _coordinateRenderer = new Coordinate2DRenderer(new BaseRenderer[]
                { LineSeriesRenderer })
            {
                BackgroundColor = new Color4(color.A, color.R, color.G, color.B),
                AutoYAxisEnable = (bool)AxisYOption.IsAutoSizeProperty.DefaultMetadata.DefaultValue,
                DefaultAxisYRange = (ScrollRange)DefaultYRangeProperty.DefaultMetadata.DefaultValue,
                TargetXRange = defaultRange,
                TargetYRange = defaultRange,
            };
            _coordinateRenderer.ActualYRangeChanged += OnRendererHostAutoAxisYCompleted;
            _renderControl = new BitmapOpenTkControl()
            {
                IsAutoAttach = true,
                IsRenderContinuously = true,
                IsShowFps = true,
                LifeCycle = ControlLifeCycle.BoundToWindow,
                RenderSetting = new RenderSetting() { RenderTactic = RenderTactic.LatencyPriority },
                Renderer = _coordinateRenderer,
            };
            _renderControl.RenderErrorReceived += OpenTkControlOnRenderErrorReceived;
            _renderControl.OpenGlErrorReceived += OpenTkControlOnOpenGlErrorReceived;
            OpenTKControl = _renderControl;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scaleElement = GetTemplateChild(SelectScaleElement) as MouseSelect;
            Debug.Assert(_scaleElement != null, nameof(_scaleElement) + " != null");
            _scaleElement.Selected += scaleElement_Scaled;
            _toolTip = GetTemplateChild(ToolTipName) as ToolTip;
            _toolTip!.PlacementTarget = this;
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            if (_scaleElement != null)
            {
                _scaleElement.Selected -= scaleElement_Scaled;
            }

            base.OnTemplateChanged(oldTemplate, newTemplate);
        }

        #region render event handler

        private static void DefaultYRangeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Series2DChart lineChart)
            {
                lineChart._coordinateRenderer.DefaultAxisYRange = (ScrollRange)e.NewValue;
            }
        }

        private static void BackgroundColorChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is Series2DChart lineChart)
            {
                var color = (Color)e.NewValue;
                lineChart._coordinateRenderer.BackgroundColor = new Color4(color.A, color.R, color.G, color.B);
            }
        }

        protected virtual void OnRendererHostAutoAxisYCompleted(ScrollRange region)
        {
            this.Dispatcher.InvokeAsync(() => { this.AxisYOption.ActualViewRange = region; });
        }

        private static void OpenTkControlOnOpenGlErrorReceived(object? sender, OpenGlErrorArgs e)
        {
            Trace.WriteLine(e.ToString());
        }

        private static void OpenTkControlOnRenderErrorReceived(object? sender, RenderErrorArgs e)
        {
            Trace.WriteLine($"{e.Exception.Message}");
        }

        #endregion

        public virtual void AttachWindow(Window hostWindow)
        {
            this._renderControl.Start(hostWindow);
        }

        public virtual void DetachWindow()
        {
            this._renderControl.Close();
        }

        #region interaction

        private Point _startMovePoint;

        private Region2D _startMoveView;

        private bool _isAutoSizeEnableCache;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _isAutoSizeEnableCache = _coordinateRenderer.AutoYAxisEnable;
            _coordinateRenderer.AutoYAxisEnable = false;
            _startMovePoint = e.GetPosition(_renderControl);
            _startMoveView = this.ActualRegion;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (_startMovePoint.Equals(e.GetPosition(_renderControl)))
            {
                _coordinateRenderer.AutoYAxisEnable = _isAutoSizeEnableCache;
            }

            AxisYOption.IsAutoSize = _coordinateRenderer.AutoYAxisEnable;
        }

        private ToolTip? _toolTip;

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (_toolTip?.IsOpen == true)
            {
                _toolTip.IsOpen = false;
            }
        }

        private readonly CollisionGrid2D _nodeGrid = new CollisionGrid2D();

        private IGeometry? _toolTipNode;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var position = e.GetPosition(_renderControl); //像素
            var coordinateRegion = this.ActualRegion;
            var winToGlMapping =
                new WindowsGlCoordinateMapping(coordinateRegion, new Rect(_renderControl.RenderSize));
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var xPixel = _startMovePoint.X - position.X;
                var xOffset = winToGlMapping.GetXOffset(xPixel);
                var yPixel = _startMovePoint.Y - position.Y;
                var yOffset = winToGlMapping.GetYOffset(-yPixel);
                var @new = _startMoveView.OffsetNew(xOffset, yOffset);
                AxisXOption.TryMoveView(@new.XRange);
                AxisYOption.TryMoveView(@new.YRange);
            }
            else
            {
                if (_toolTip == null)
                {
                    return;
                }

                var mapGlPoint = winToGlMapping.GetGlPointByWindowsPoint(position);
                var xDistance = winToGlMapping.XScaleRatio * 10;
                var yDistance = winToGlMapping.YScaleRatio * 10;
                var ellipse = new MouseCollisionEllipse(mapGlPoint, (float)xDistance, (float)yDistance);
                if (_nodeGrid.TrySearch(ellipse, out var geometry, out var layer))
                {
                    if (_toolTipNode?.Equals(geometry) != true && geometry != null)
                    {
                        _toolTipNode = geometry;
                        // var winPoint = winToGlMapping.GetWindowsPointByGlPoint(geometry.Point);
                        var seriesItem = _items.First(item =>
                            item.CollisionLayer.Equals(layer));
                        _toolTip.HorizontalOffset = position.X + 10;
                        _toolTip.VerticalOffset = position.Y + 10;
                        _toolTip.Content = new MouseHoverNodeData(seriesItem.Color, geometry,
                            seriesItem.Title);
                    }

                    if (!_toolTip.IsOpen)
                    {
                        _toolTip.IsOpen = true;
                    }

                    return;
                }
            }

            if (_toolTip?.IsOpen == true)
            {
                _toolTip.IsOpen = false;
            }
        }

        private void scaleElement_Scaled(object? sender, SelectionArgs e)
        {
            var oldRegion = this.ActualRegion;
            var scale = new WindowsGlCoordinateMapping(oldRegion, e.FullRect);
            scale.ScaleByRect(e.SelectRect, out var xRange, out var yRange);
            AxisXOption.TryScaleView(xRange);
            AxisYOption.TryScaleView(yRange);
            AxisYOption.IsAutoSize = false;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            AxisYOption.IsAutoSize = false;
            base.OnMouseWheel(e);
            var axisXOption = AxisXOption;
            var region = this.ActualRegion;
            var newXRange = region.XRange;
            var newYRange = region.YRange;
            if (axisXOption.ZoomEnable)
            {
                newXRange = e.Scale(newXRange);
                axisXOption.TryScaleView(newXRange);
            }

            var axisYOption = AxisYOption;
            if (axisYOption.ZoomEnable)
            {
                newYRange = e.Scale(newYRange);
                axisYOption.TryScaleView(newYRange);
            }
        }

        #endregion

        #region collection

        protected readonly Line2DSeriesRenderer LineSeriesRenderer =
            new Line2DSeriesRenderer(new Shader("Render/Shaders/LineShader/shader.vert",
                "Render/Shaders/LineShader/shader.frag"));

        private readonly List<ISeries2D> _items = new List<ISeries2D>(5);

        public IReadOnlyList<ISeries2D> SeriesItems =>
            new ReadOnlyCollection<ISeries2D>(_items);

        public T NewSeries<T>() where T : ISeries2D
        {
            var collisionSeed = this.CollisionSeed;
            ICollisionPoint2D collisionPoint2D;
            switch (CollisionEnum)
            {
                case CollisionEnum.SpacialHash:
                    collisionPoint2D = new SpacialHashCollisionPoint2DLayer(collisionSeed.XSpan,
                        SpacialHashCollisionPoint2DLayer.Algorithm.XMapping,
                        (int)InitialCollisionGridBoundary.XSpan);
                    break;
                case CollisionEnum.UniformGrid:
                    collisionPoint2D = new CollisionGridPoint2DLayer(InitialCollisionGridBoundary,
                        collisionSeed.XSpan, collisionSeed.YSpan, new LinkedListGridCellFactory());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var type = typeof(T);
            if (type == typeof(RingLine2DRenderer))
            {
                var lineRenderer = new RingLine2DRenderer(collisionPoint2D);
                this._nodeGrid.AddLayer(lineRenderer.CollisionLayer);
                this.LineSeriesRenderer.Add(lineRenderer);
                this._items.Add(lineRenderer);
                return (T)(ISeries2D)lineRenderer;
            }

            throw new NotSupportedException();
        }

        public void Remove(ISeries2D series)
        {
            if (series is RingLine2DRenderer lineRenderer)
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