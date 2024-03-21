using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using GLChart.WPF.Base;
using GLChart.WPF.Render;
using GLChart.WPF.Render.CollisionDetection;
using GLChart.WPF.Render.Renderer;
using GLChart.WPF.UIComponent.Control;
using GLChart.WPF.UIComponent.Interaction;
using OpenTkWPFHost.Control;

namespace GLChart.Samples.Charts;

[TemplatePart(Name = SliderName, Type = typeof(ContentRangeSlider))]
[TemplatePart(Name = ThumbnailElementName, Type = typeof(OpenTKSubControl))]
public class HistoricalChart : Chart2DCore
{
    public const string SliderName = "RangeSlider";
    public const string ThumbnailElementName = "ThumbnailElement";

    static HistoricalChart()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HistoricalChart),
            new FrameworkPropertyMetadata(typeof(HistoricalChart)));
    }

    public HistoricalChart()
    {
        this.Loaded += HistoricalLiveChart_Loaded;
        this.Unloaded += HistoricalLiveChart_Unloaded;
    }

    private bool _hasLoaded;

    private void HistoricalLiveChart_Loaded(object sender, RoutedEventArgs e)
    {
        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;
    }

    /// <summary>
    /// 由于可能未第一时间加载，<see cref="OnApplyTemplate"/> 不会被调用，导致控件变量为null，所以在load后再监听所有行为
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HistoricalLiveChart_Unloaded(object sender, RoutedEventArgs e)
    {
        if (!_hasLoaded)
        {
            return;
        }
    }

    #region interaction

    /// <summary>
    /// 交互模式
    /// </summary>
    public static readonly DependencyProperty InteractModeProperty = DependencyProperty.Register(
        nameof(InteractMode), typeof(ChartInteractMode), typeof(HistoricalChart),
        new PropertyMetadata(ChartInteractMode.AutoAll));

    /// <summary>
    /// 交互模式
    /// </summary>
    public ChartInteractMode InteractMode
    {
        get { return (ChartInteractMode)GetValue(InteractModeProperty); }
        set { SetValue(InteractModeProperty, value); }
    }


    /// <summary>
    /// 设置点位的x轴范围
    /// </summary>
    /// <param name="range"></param>
    /* private void SetPointsAxisXRange(ScrollRange range)
    {
        ReSetByInteractMode(this.InteractMode, range);
    }

    private ScrollRange _pointRange;

    //todo:
    public void ReSetInteractMode(ChartInteractMode mode)
    {
        ReSetByInteractMode(mode, _pointRange);
    }

    //todo：适配dp属性
   public void ReSetByInteractMode(ChartInteractMode mode, ScrollRange pointRange)
    {
        var pointEqual = _pointRange.Equals(pointRange);
        if (pointEqual && mode.Equals(this.InteractMode))
        {
            return;
        }

        if (!pointEqual)
        {
            _pointRange = pointRange;
        }

        if (mode == ChartInteractMode.Manual)
        {
            //手动模式下不对点位区间响应
            if (!pointEqual)
            {
                this.AxisXBoundary = pointRange;
            }

            return;
        }

        if (!this._contentChart.IsAutoYAxisEnable)
        {
            this._contentChart.IsAutoYAxisEnable = true;
        }

        _contentChart.DisplayRegion.SetBottom(this.AxisYOption.ZoomBoundary.Start);
        var pointRangeEnd = this._pointRange.End;
        var pointRangeStart = this._pointRange.Start;
        var xRange = this._contentChart.DisplayRegion.XRange;
        if (mode == ChartInteractMode.AutoAll)
        {
            var fixedMarginValue = this._pointRange.Range * AutoAllPercentageMargin;
            if (fixedMarginValue < AutoAllMinMargin)
            {
                fixedMarginValue = AutoAllMinMargin;
            }

            var pointAxisX = fixedMarginValue + pointRangeEnd;
            xRange = new ScrollRange(pointRangeStart, pointAxisX);
            this.AxisXBoundary = xRange;
        }
        else if (mode == ChartInteractMode.AutoScroll)
        {
            var maximum = pointRangeEnd + AutoScrollMargin;
            var scrollRange = this.ScrollBarViewRange;
            var rangeStart = scrollRange.Start;
            if (maximum - rangeStart > ScrollWindow)
            {
                var rangeLowerValue = maximum - ScrollWindow;
                xRange = new ScrollRange(rangeLowerValue, maximum);
            }
            else
            {
                xRange = xRange.WithEnd(maximum);
            }

            this.AxisXBoundary = this.AxisXBoundary.Merge(xRange);
        }

        this.ScrollBarViewRange = xRange;
        this.InteractMode = mode;
    }

    /// <summary>
    /// 重设轴
    /// </summary>
    public void ResetAxisX()
    {
        ReSetByInteractMode(ChartInteractMode.AutoAll, this.AxisXOption.ZoomBoundary);
    }*/

    #endregion

    #region series

    public static readonly DependencyProperty LinesProperty = DependencyProperty.Register(
        nameof(Lines), typeof(IList<ILine2D>), typeof(HistoricalChart),
        new PropertyMetadata(default(IList<ILine2D>)));

    public IList<ILine2D> Lines
    {
        get { return (IList<ILine2D>)GetValue(LinesProperty); }
        set { SetValue(LinesProperty, value); }
    }

    public static readonly DependencyProperty MaxPointsCountLimitProperty = DependencyProperty.Register(
        "MaxPointsCountLimit", typeof(int), typeof(HistoricalChart), new PropertyMetadata(10000));

    public int MaxPointsCountLimit
    {
        get { return (int)GetValue(MaxPointsCountLimitProperty); }
        set { SetValue(MaxPointsCountLimitProperty, value); }
    }

    #endregion

    public bool IsHistoryVisible
    {
        get { return (bool)GetValue(IsHistoryVisibleProperty); }
        set { SetValue(IsHistoryVisibleProperty, value); }
    }

    public static readonly DependencyProperty IsHistoryVisibleProperty =
        DependencyProperty.Register("IsHistoryVisible", typeof(bool), typeof(HistoricalChart),
            new PropertyMetadata(true));

    #region Axis

    public static readonly DependencyProperty ScrollWindowProperty = DependencyProperty.Register(
        "ScrollWindow", typeof(double), typeof(HistoricalChart), new PropertyMetadata(default(double)));

    public double ScrollWindow
    {
        get { return (double)GetValue(ScrollWindowProperty); }
        set { SetValue(ScrollWindowProperty, value); }
    }


    public static readonly DependencyProperty AutoScrollMarginProperty = DependencyProperty.Register(
        "AutoScrollMargin", typeof(double), typeof(HistoricalChart), new PropertyMetadata(default));

    /// <summary>
    /// 固定值，因为自动滚动模式的<see cref="ScrollWindow"/>是固定值
    /// </summary>
    public double AutoScrollMargin
    {
        get { return (double)GetValue(AutoScrollMarginProperty); }
        set { SetValue(AutoScrollMarginProperty, value); }
    }

    public static readonly DependencyProperty AutoAllPercentageMarginProperty = DependencyProperty.Register(
        "AutoAllPercentageMargin", typeof(double), typeof(HistoricalChart),
        new PropertyMetadata(0d));

    /// <summary>
    /// 百分比值的全域模式空白
    /// </summary>
    public double AutoAllPercentageMargin
    {
        get { return (double)GetValue(AutoAllPercentageMarginProperty); }
        set { SetValue(AutoAllPercentageMarginProperty, value); }
    }

    public static readonly DependencyProperty AutoAllMinMarginProperty = DependencyProperty.Register(
        "AutoAllMinMargin", typeof(double), typeof(HistoricalChart), new PropertyMetadata(0d));

    /// <summary>
    /// 最小的空白间隔，对<see cref="AutoAllPercentageMargin"/>负责，当计算出的margin小于该值时采纳该值
    /// </summary>
    public double AutoAllMinMargin
    {
        get { return (double)GetValue(AutoAllMinMarginProperty); }
        set { SetValue(AutoAllMinMarginProperty, value); }
    }

    public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(
        "TickFrequency", typeof(double), typeof(HistoricalChart), new PropertyMetadata(1d));

    public double TickFrequency
    {
        get { return (double)GetValue(TickFrequencyProperty); }
        set { SetValue(TickFrequencyProperty, value); }
    }

    public static readonly DependencyProperty IsSnapToTickEnabledProperty = DependencyProperty.Register(
        "IsSnapToTickEnabled", typeof(bool), typeof(HistoricalChart), new PropertyMetadata(false));

    public bool IsSnapToTickEnabled
    {
        get { return (bool)GetValue(IsSnapToTickEnabledProperty); }
        set { SetValue(IsSnapToTickEnabledProperty, value); }
    }

    #endregion

    #region label

    /*public static readonly DependencyProperty XYMapperProperty = DependencyProperty.Register(
        "XYMapper", typeof(IMapper), typeof(HistoricalGlChart), new PropertyMetadata(default(IMapper)));

    public IMapper XYMapper
    {
        get { return (IMapper)GetValue(XYMapperProperty); }
        set { SetValue(XYMapperProperty, value); }
    }*/

    public static readonly DependencyProperty LegendVisibilityProperty = DependencyProperty.Register(
        "LegendVisibility", typeof(Visibility), typeof(HistoricalChart),
        new PropertyMetadata(Visibility.Visible));

    public Visibility LegendVisibility
    {
        get { return (Visibility)GetValue(LegendVisibilityProperty); }
        set { SetValue(LegendVisibilityProperty, value); }
    }

    public DataTemplate LegendTemplate
    {
        get { return (DataTemplate)GetValue(LegendTemplateProperty); }
        set { SetValue(LegendTemplateProperty, value); }
    }

    public static readonly DependencyProperty LegendTemplateProperty =
        DependencyProperty.Register("LegendTemplate", typeof(DataTemplate), typeof(HistoricalChart),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ToolTipTemplateProperty = DependencyProperty.Register(
        "ToolTipTemplate", typeof(DataTemplate), typeof(HistoricalChart),
        new PropertyMetadata(default(DataTemplate)));

    public DataTemplate ToolTipTemplate
    {
        get { return (DataTemplate)GetValue(ToolTipTemplateProperty); }
        set { SetValue(ToolTipTemplateProperty, value); }
    }

    public static readonly DependencyProperty RangeSliderAutoToolTipValueTemplateProperty =
        DependencyProperty.Register(
            "RangeSliderAutoToolTipValueTemplate", typeof(DataTemplate), typeof(HistoricalChart),
            new PropertyMetadata(default(DataTemplate)));

    public DataTemplate RangeSliderAutoToolTipValueTemplate
    {
        get { return (DataTemplate)GetValue(RangeSliderAutoToolTipValueTemplateProperty); }
        set { SetValue(RangeSliderAutoToolTipValueTemplateProperty, value); }
    }

    public static readonly DependencyProperty RangeSliderAutoToolTipRangeValuesTemplateProperty =
        DependencyProperty.Register(
            "RangeSliderAutoToolTipRangeValuesTemplate", typeof(DataTemplate), typeof(HistoricalChart),
            new PropertyMetadata(default(DataTemplate)));

    public DataTemplate RangeSliderAutoToolTipRangeValuesTemplate
    {
        get { return (DataTemplate)GetValue(RangeSliderAutoToolTipRangeValuesTemplateProperty); }
        set { SetValue(RangeSliderAutoToolTipRangeValuesTemplateProperty, value); }
    }

    #endregion

    private ContentRangeSlider? _rangeSlider;

    private OpenTKSubControl? _historicalChart;

    private Coordinate2DRendererView? _overViewRenderer;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        // _contentChart.ChangeRegionRequest += _contentChart_ScaleRequest;
        _historicalChart = GetTemplateChild(ThumbnailElementName) as OpenTKSubControl;
        _overViewRenderer = (Coordinate2DRendererView)_historicalChart!.Renderer;
        _overViewRenderer.Bind(CoordinateRenderer!);
        this.CoordinateRenderer!.SeriesRenderers.Add(_ringLine2DSeriesRenderer);
        _rangeSlider = GetTemplateChild(SliderName) as ContentRangeSlider;
    }

    #region Line

    private readonly RingLine2DSeriesRenderer _ringLine2DSeriesRenderer
        = new RingLine2DSeriesRenderer(new Shader("Render/Shaders/LineShader/shader.vert",
            "Render/Shaders/LineShader/shader.frag"));

    /// <summary>
    /// “碰撞种子” ,影响碰撞检测的性能
    /// </summary>
    public Boundary2D CollisionSeed { get; set; } = new Boundary2D(0, 100, 0, 100);

    /// <summary>
    /// 初始化碰撞检测的边界，减少碰撞网格的分配开销
    /// </summary>
    public Boundary2D InitialCollisionGridBoundary { get; set; } = new Boundary2D(0, 100, 0, 100);

    /// <summary>
    /// 创建历史线
    /// </summary>
    /// <returns></returns>
    public ILine2D NewLine(int maxPointCount)
    {
        var point2DLayer = new SpacialHashCollisionPoint2DLayer(CollisionSeed.XSpan,
            SpacialHashCollisionPoint2DLayer.Algorithm.XMapping,
            (int)InitialCollisionGridBoundary.XSpan);
        var lineRenderer = new RingLine2DRenderer(point2DLayer, maxPointCount);
        this.CollisionGrid.AddLayer(lineRenderer.CollisionLayer);
        this._ringLine2DSeriesRenderer.Add(lineRenderer);
        this.Series2Ds.Add(lineRenderer);
        return lineRenderer;
    }

    public void Remove(ILine2D series)
    {
        if (series is RingLine2DRenderer lineRenderer)
        {
            this.Series2Ds.Remove(lineRenderer);
            this._ringLine2DSeriesRenderer.Remove(lineRenderer);
            this.CollisionGrid.Remove(lineRenderer.CollisionLayer);
        }
    }

    public void Clear()
    {
        this.Series2Ds.Clear();
        this._ringLine2DSeriesRenderer.Clear();
        this.CollisionGrid.Clear();
    }

    #endregion


    //todo:
    /*private void _contentChart_ScaleRequest(CoordinateRegion obj)
    {
        this.InteractMode = ChartInteractMode.Manual;
        this.ScrollBarViewRange = obj.XRange;
    }*/

    /*private void RangeSlider_RangeSelectionChanged(object sender,
        MahApps.Metro.Controls.RangeSelectionChangedEventArgs<double> e)
    {
        //直接同步以减少调用开销
        _scrollBarView = new ScrollRange(e.NewLowerValue, e.NewUpperValue);
        var displayRegion = _contentChart.DisplayRegion;
        if (this.InteractMode != ChartInteractMode.Manual) //当来自自动模式的请求时，重置Y轴
        {
            displayRegion.SetBottom(this.BindingSource.AxisY.Boundary.Start);
        }

        _contentChart.DisplayRegion =
            displayRegion.ChangeXRange(_scrollBarView);
        InteractMode = ChartInteractMode.Manual;
    }*/
}