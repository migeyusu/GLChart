using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using OpenTkWPFHost.Control;
using OpenTkWPFHost.Core;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;

namespace GLChart.WPF.UIComponent.Control
{
    public class Series2DChart : Chart2DCore, ISeries2DChart
    {
        static Series2DChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Series2DChart),
                new FrameworkPropertyMetadata(typeof(Series2DChart)));
        }

        #region collection

        public IReadOnlyList<ISeries2D> SeriesItems =>
            new ReadOnlyCollection<ISeries2D>(Series2Ds);

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
                this.CollisionGrid.AddLayer(lineRenderer.CollisionLayer);
                this.Series2Ds.Add(lineRenderer);
                return (T)(ISeries2D)lineRenderer;
            }

            throw new NotSupportedException();
        }

        public void Remove(ISeries2D series)
        {
            if (series is RingLine2DRenderer lineRenderer)
            {
                this.Series2Ds.Remove(lineRenderer);
                this.CollisionGrid.Remove(lineRenderer.CollisionLayer);
            }
        }

        public void Clear()
        {
            this.Series2Ds.Clear();
            this.CollisionGrid.Clear();
        }

        #endregion
    }

    /// <summary>
    /// 融合了Renderer和Coordinate2D的基础图
    /// </summary>
    [TemplatePart(Name = ToolTipName, Type = typeof(ToolTip))]
    [TemplatePart(Name = CoordinateElementName, Type = typeof(Coordinate2D))]
    [TemplatePart(Name = ThreadOpenTkControl, Type = typeof(BitmapOpenTkControl))]
    [TemplatePart(Name = SelectScaleElement, Type = typeof(MouseSelect))]
    [TemplatePart(Name = Coordinate2DRendererName, Type = typeof(Coordinate2DRenderer))]
    public class Chart2DCore : System.Windows.Controls.Control
    {
        private const string CoordinateElementName = "Coordinate";

        private const string SelectScaleElement = "SelectScaleElement";

        private const string ToolTipName = "ToolTip";

        private const string ThreadOpenTkControl = "OpenTkControl";

        private const string Coordinate2DRendererName = "Coordinate2DRenderer";

        static Chart2DCore()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Chart2DCore),
                new FrameworkPropertyMetadata(typeof(Chart2DCore)));
        }

        public static readonly DependencyProperty ToolTipTemplateProperty = DependencyProperty.Register(
            nameof(ToolTipTemplate), typeof(DataTemplate), typeof(Chart2DCore),
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
            nameof(AxisXOption), typeof(AxisXOption), typeof(Chart2DCore),
            new FrameworkPropertyMetadata(default));

        public AxisXOption AxisXOption
        {
            get => (AxisXOption)GetValue(AxisXOptionProperty);
            set => SetValue(AxisXOptionProperty, value);
        }

        public static readonly DependencyProperty AxisYOptionProperty = DependencyProperty.Register(
            nameof(AxisYOption), typeof(AxisYOption), typeof(Chart2DCore),
            new FrameworkPropertyMetadata(default));

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

        #endregion

        #region render

        private BitmapOpenTkControl? _renderControl;

        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
            nameof(BackgroundColor), typeof(Color), typeof(Chart2DCore),
            new PropertyMetadata(Colors.White));

        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public static readonly DependencyProperty IsShowFpsProperty = DependencyProperty.Register(
            nameof(IsShowFps), typeof(bool), typeof(Chart2DCore), new PropertyMetadata(default(bool)));

        public bool IsShowFps
        {
            get => (bool)GetValue(IsShowFpsProperty);
            set => SetValue(IsShowFpsProperty, value);
        }

        public Coordinate2DRenderer? Renderer => _coordinateRenderer;

        private Coordinate2DRenderer? _coordinateRenderer;

        #endregion

        private MouseSelect? _scaleElement;

        public Chart2DCore()
        {
        }
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scaleElement = GetTemplateChild(SelectScaleElement) as MouseSelect;
            Debug.Assert(_scaleElement != null, nameof(_scaleElement) + " != null");
            _scaleElement.Selected += scaleElement_Scaled;
            _toolTip = GetTemplateChild(ToolTipName) as ToolTip;
            _toolTip!.PlacementTarget = this;
            _coordinateRenderer = GetTemplateChild(Coordinate2DRendererName) as Coordinate2DRenderer;
            _renderControl = GetTemplateChild(ThreadOpenTkControl) as BitmapOpenTkControl;
            _renderControl!.RenderErrorReceived += OpenTkControlOnRenderErrorReceived;
            _renderControl.OpenGlErrorReceived += OpenTkControlOnOpenGlErrorReceived;
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            if (_scaleElement != null)
            {
                _scaleElement.Selected -= scaleElement_Scaled;
            }

            if (_renderControl != null)
            {
                _renderControl.RenderErrorReceived -= OpenTkControlOnRenderErrorReceived;
                _renderControl.OpenGlErrorReceived -= OpenTkControlOnOpenGlErrorReceived;
            }

            base.OnTemplateChanged(oldTemplate, newTemplate);
        }

        #region render event handler

        private static void OpenTkControlOnOpenGlErrorReceived(object? sender, OpenGlErrorArgs e)
        {
            Trace.WriteLine(e.ToString());
        }

        private static void OpenTkControlOnRenderErrorReceived(object? sender, RenderErrorArgs e)
        {
            Trace.WriteLine($"{e.Exception.Message}");
        }

        #endregion

        #region interaction

        private Point _startMovePoint;

        private Region2D _startMoveView;

        private bool _isAutoSizeEnableCache;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _isAutoSizeEnableCache = _coordinateRenderer!.AutoYAxisEnable;
            _coordinateRenderer.AutoYAxisEnable = false;
            _startMovePoint = e.GetPosition(_renderControl);
            _startMoveView = this.ActualRegion;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (_startMovePoint.Equals(e.GetPosition(_renderControl)))
            {
                _coordinateRenderer!.AutoYAxisEnable = _isAutoSizeEnableCache;
            }

            AxisYOption.IsAutoSize = _coordinateRenderer!.AutoYAxisEnable;
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

        protected readonly CollisionGrid2D CollisionGrid = new CollisionGrid2D();

        private IGeometry? _toolTipNode;

        protected readonly List<ISeries2D> Series2Ds = new List<ISeries2D>(5);

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var position = e.GetPosition(_renderControl); //像素
            var coordinateRegion = this.ActualRegion;
            var winToGlMapping =
                new WindowsGlCoordinateMapping(coordinateRegion, new Rect(_renderControl!.RenderSize));
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
                if (CollisionGrid.TrySearch(ellipse, out var geometry, out var layer))
                {
                    if (_toolTipNode?.Equals(geometry) != true && geometry != null)
                    {
                        _toolTipNode = geometry;
                        // var winPoint = winToGlMapping.GetWindowsPointByGlPoint(geometry.Point);
                        var seriesItem = Series2Ds.First(item =>
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
    }
}