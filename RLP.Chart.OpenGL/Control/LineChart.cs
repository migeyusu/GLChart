using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using OpenTkWPFHost.Control;
using RLP.Chart.Interface;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.CollisionDetection;
using RLP.Chart.OpenGL.Interaction;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.Control
{
    /// <summary>
    /// 基于2d xy坐标系的图
    /// </summary>
    [TemplatePart(Name = ThreadOpenTkControl, Type = typeof(Coordinate2D))]
    [TemplatePart(Name = ThreadOpenTkControl, Type = typeof(ThreadOpenTkControl))]
    [TemplatePart(Name = SelectScaleElement, Type = typeof(MouseSelect))]
//    [TemplatePart(Name = Popup, Type = typeof(ToolTip))]
    public class LineChart : LineChartBase
    {
        public static readonly DependencyProperty CollisionEnumProperty = DependencyProperty.Register(
            "CollisionEnum", typeof(CollisionEnum), typeof(LineChart),
            new PropertyMetadata(CollisionEnum.SpacialHash));

        public CollisionEnum CollisionEnum
        {
            get { return (CollisionEnum) GetValue(CollisionEnumProperty); }
            set { SetValue(CollisionEnumProperty, value); }
        }

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
            get { return (DataTemplate) GetValue(ToolTipTemplateProperty); }
            set { SetValue(ToolTipTemplateProperty, value); }
        }

        public static readonly DependencyProperty CollisionSeedProperty = DependencyProperty.Register(
            "CollisionSeed", typeof(Boundary2D), typeof(LineChart),
            new PropertyMetadata(new Boundary2D(0, 100, 0, 100)));

        /// <summary>
        /// “碰撞种子” ,决定了碰撞检测的性能
        /// </summary>
        public Boundary2D CollisionSeed
        {
            get { return (Boundary2D) GetValue(CollisionSeedProperty); }
            set { SetValue(CollisionSeedProperty, value); }
        }

        public static readonly DependencyProperty InitialCollisionGridProperty = DependencyProperty.Register(
            "InitialCollisionGrid", typeof(Boundary2D), typeof(LineChart),
            new PropertyMetadata(new Boundary2D(0, 100, 0, 100)));

        /// <summary>
        /// 初始化碰撞检测的边界，减少碰撞网格的分配开销
        /// </summary>
        public Boundary2D InitialCollisionGrid
        {
            get { return (Boundary2D) GetValue(InitialCollisionGridProperty); }
            set { SetValue(InitialCollisionGridProperty, value); }
        }

        public static readonly DependencyProperty XLabelGenerationOptionProperty = DependencyProperty.Register(
            "XLabelGenerationOption", typeof(LabelGenerationOption), typeof(LineChart),
            new PropertyMetadata(default(LabelGenerationOption)));

        public LabelGenerationOption XLabelGenerationOption
        {
            get { return (LabelGenerationOption) GetValue(XLabelGenerationOptionProperty); }
            set { SetValue(XLabelGenerationOptionProperty, value); }
        }

        public static readonly DependencyProperty YLabelGenerationOptionProperty = DependencyProperty.Register(
            "YLabelGenerationOption", typeof(LabelGenerationOption), typeof(LineChart),
            new PropertyMetadata(default(LabelGenerationOption)));

        public LabelGenerationOption YLabelGenerationOption
        {
            get { return (LabelGenerationOption) GetValue(YLabelGenerationOptionProperty); }
            set { SetValue(YLabelGenerationOptionProperty, value); }
        }


        /// <summary>
        /// virtual boundary, to limit scroll range
        /// </summary>
        public ScrollRange AxisXScrollBoundary { get; set; }

        public float MinXRange { get; set; }

        public float MinYRange { get; set; }

        /// <summary>
        /// 自适应Y轴的默认区间，当界面内没有元素时显示该区间
        /// </summary>
        public ScrollRange DefaultAutoSizeAxisYRange { get; set; }

        private MouseSelect _scaleElement;

        private readonly CollisionGrid _nodeGrid = new CollisionGrid();

        private ToolTip _toolTip;

        private Node _popupNode = default;

        public LineChart() : base()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
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

        private System.Windows.Point _startMovePoint;

        private bool _mouseLeftButtonPressed;

        private Region2D _startMoveRegion;

        private void _scaleElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mouseLeftButtonPressed = false;
        }

        private void _scaleElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseLeftButtonPressed = true;
            _startMovePoint = e.GetPosition(OpenTkControl);
            _startMoveRegion = this.DisplayRegion;
        }

        private void _scaleElement_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseLeftButtonPressed = false;
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
            if (_mouseLeftButtonPressed)
            {
                var xPixel = _startMovePoint.X - position.X;
                var xOffset = windowsRectToGlMapping.GetXOffset(xPixel);
                double yOffset = 0;
                if (!AutoYAxis)
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
                var ellipseGeometry = new Ellipse(mapGlPoint, (float) xDistance, (float) yDistance);
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

        #region scale

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
            if (this.AutoYAxis)
            {
                this.AutoYAxis = false;
            }

            if (newRegion.Height < MinYRange)
            {
                newRegion.ChangeYRange(oldRegion.YRange);
            }

            if (newRegion.Width < MinXRange)
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
            if (newRange.Range < this.MinXRange)
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

        private readonly List<SeriesItem> _items = new List<SeriesItem>(5);

        public override IReadOnlyList<ILineSeries> SeriesItems => new ReadOnlyCollection<SeriesItem>(_items);

        public override ILineSeries NewSeries()
        {
            var collisionSeed = this.CollisionSeed;
            switch (CollisionEnum)
            {
                case CollisionEnum.SpacialHash:
                    return new SeriesItem(new SpacialHashSet(collisionSeed.XSpan, SpacialHashSet.Algorithm.XMapping,
                        (int) InitialCollisionGrid.XSpan));
                case CollisionEnum.UniformGrid:
                    var collisionGridLayer = new CollisionGridLayer(InitialCollisionGrid,
                        collisionSeed.XSpan, collisionSeed.YSpan, new LinkedListGridCellFactory());
                    return new SeriesItem(collisionGridLayer);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Add(ILineSeries item)
        {
            if (item is SeriesItem seriesItem)
            {
                this._nodeGrid.AddLayer(seriesItem.CollisionGridLayer);
                this.LineSeriesRenderer.Add(seriesItem.Renderer);
                this._items.Add(seriesItem);
            }
        }

        public override void Remove(ILineSeries seriesItem)
        {
            var find = _items.Find((item => item.Id == seriesItem.Id));
            this._items.Remove(find);
            this.LineSeriesRenderer.Remove(find.Renderer);
            this._nodeGrid.Remove(find.CollisionGridLayer);
        }

        public override void Clear()
        {
            this.LineSeriesRenderer.Clear();
            this._items.Clear();
            this._nodeGrid.Clear();
        }

        /// <summary>
        /// 在推送点位上有两种方式：1.从推送目标关联创建
        /// 2. 独立的集合，附加到目标
        /// </summary>
        public class SeriesItem : SeriesItemLight
        {
            public SeriesItem(ICollisionLayer collisionGridLayer)
            {
                CollisionGridLayer = collisionGridLayer;
            }

            public ICollisionLayer CollisionGridLayer { get; }

            public override void AddGeometry(IPoint2D point)
            {
                base.AddGeometry(point);
                CollisionGridLayer.AddNode(point);
            }

            public override void AddGeometries(IList<IPoint2D> points)
            {
                base.AddGeometries(points);
                CollisionGridLayer.AddNodes(points);
            }

            public override void ResetWith(IPoint2D geometry)
            {
                base.ResetWith(geometry);
                CollisionGridLayer.ResetWithNode(geometry);
            }

            public override void ResetWith(IList<IPoint2D> geometries)
            {
                base.ResetWith(geometries);
                CollisionGridLayer.ResetWithNodes(geometries);
            }

            public override void Clear()
            {
                base.Clear();
                this.CollisionGridLayer.ClearNodes();
            }
        }
    }
}