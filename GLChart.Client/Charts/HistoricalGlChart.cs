using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GLChart.WPF.Base;
using GLChart.WPF.UIComponent.Axis;
using GLChart.WPF.UIComponent.Control;

namespace GLChart.Samples.Charts;

[TemplatePart(Name = SliderName, Type = typeof(ContentRangeSlider))]
[TemplatePart(Name = ContentChart, Type = typeof(Series2DChart))]
[TemplatePart(Name = ThumbnailElementName, Type = typeof(DrawableElement))]
public class HistoricalGlChart : Control
{
    public const string SliderName = "RangeSlider";
    public const string ContentChart = "ContentChart";
    public const string ThumbnailElementName = "ThumbnailElement";

    static HistoricalGlChart()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HistoricalGlChart),
            new FrameworkPropertyMetadata(typeof(HistoricalGlChart)));
    }

    public HistoricalGlChart()
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
        nameof(InteractMode), typeof(ChartInteractMode), typeof(HistoricalGlChart),
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
    private void SetPointsAxisXRange(ScrollRange range)
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

        this.InteractMode = mode;
        if (!pointEqual)
        {
            _pointRange = pointRange;
        }

        this.InteractMode = mode;
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
    }

    #endregion

    #region series

    public static readonly DependencyProperty LinesProperty = DependencyProperty.Register(
        nameof(Lines), typeof(IList<ILine2D>), typeof(HistoricalGlChart),
        new PropertyMetadata(default(IList<ILine2D>)));

    public IList<ILine2D> Lines
    {
        get { return (IList<ILine2D>)GetValue(LinesProperty); }
        set { SetValue(LinesProperty, value); }
    }

    public static readonly DependencyProperty MaxPointsCountLimitProperty = DependencyProperty.Register(
        "MaxPointsCountLimit", typeof(int), typeof(HistoricalGlChart), new PropertyMetadata(10000));

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
        DependencyProperty.Register("IsHistoryVisible", typeof(bool), typeof(HistoricalGlChart),
            new PropertyMetadata(true));

    #region Axis

    private ScrollRange _axisXBoundary;

    /// <summary>
    /// X 轴位置
    /// </summary>
    public ScrollRange AxisXBoundary
    {
        get { return _axisXBoundary; }
        set
        {
            if (_axisXBoundary.Equals(value))
            {
                return;
            }

            this._axisXBoundary = value;
            this._historicalChart.DisplayRegion = _historicalChart.DisplayRegion.ChangeXRange(value);
            this._contentChart.AxisXScrollBoundary = value;
            this._rangeSlider.Minimum = value.Start;
            this._rangeSlider.Maximum = value.End;
        }
    }

    public static readonly DependencyProperty ScrollWindowProperty = DependencyProperty.Register(
        "ScrollWindow", typeof(double), typeof(HistoricalGlChart), new PropertyMetadata(default(double)));

    public double ScrollWindow
    {
        get { return (double)GetValue(ScrollWindowProperty); }
        set { SetValue(ScrollWindowProperty, value); }
    }


    public static readonly DependencyProperty AutoScrollMarginProperty = DependencyProperty.Register(
        "AutoScrollMargin", typeof(double), typeof(HistoricalGlChart), new PropertyMetadata(default));

    /// <summary>
    /// 固定值，因为自动滚动模式的<see cref="ScrollWindow"/>是固定值
    /// </summary>
    public double AutoScrollMargin
    {
        get { return (double)GetValue(AutoScrollMarginProperty); }
        set { SetValue(AutoScrollMarginProperty, value); }
    }

    public static readonly DependencyProperty AutoAllPercentageMarginProperty = DependencyProperty.Register(
        "AutoAllPercentageMargin", typeof(double), typeof(HistoricalGlChart),
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
        "AutoAllMinMargin", typeof(double), typeof(HistoricalGlChart), new PropertyMetadata(0d));

    /// <summary>
    /// 最小的空白间隔，对<see cref="AutoAllPercentageMargin"/>负责，当计算出的margin小于该值时采纳该值
    /// </summary>
    public double AutoAllMinMargin
    {
        get { return (double)GetValue(AutoAllMinMarginProperty); }
        set { SetValue(AutoAllMinMarginProperty, value); }
    }

    private ScrollRange _scrollBarView;

    /// <summary>
    /// 滚动条，作为x轴范围输入
    /// </summary>
    private ScrollRange ScrollBarViewRange
    {
        get
        {
            return _scrollBarView;
            // return new ScrollRange(_rangeSlider.LowerValue, _rangeSlider.UpperValue);
        }
        set
        {
            if (value.Equals(_scrollBarView))
            {
                return;
            }

            this._scrollBarView = value;
            this._rangeSlider.UpperValue = value.End;
            this._rangeSlider.LowerValue = value.Start;
        }
    }

    public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(
        "TickFrequency", typeof(double), typeof(HistoricalGlChart), new PropertyMetadata(1d));

    public double TickFrequency
    {
        get { return (double)GetValue(TickFrequencyProperty); }
        set { SetValue(TickFrequencyProperty, value); }
    }

    public static readonly DependencyProperty IsSnapToTickEnabledProperty = DependencyProperty.Register(
        "IsSnapToTickEnabled", typeof(bool), typeof(HistoricalGlChart), new PropertyMetadata(true));

    public bool IsSnapToTickEnabled
    {
        get { return (bool)GetValue(IsSnapToTickEnabledProperty); }
        set { SetValue(IsSnapToTickEnabledProperty, value); }
    }

    public static readonly DependencyProperty AxisXOptionProperty = DependencyProperty.Register(
        nameof(AxisXOption), typeof(AxisOption), typeof(HistoricalGlChart), new PropertyMetadata(new AxisOption()));

    public AxisOption AxisXOption
    {
        get { return (AxisOption)GetValue(AxisXOptionProperty); }
        set { SetValue(AxisXOptionProperty, value); }
    }

    public static readonly DependencyProperty AxisYOptionProperty = DependencyProperty.Register(
        nameof(AxisYOption), typeof(AxisOption), typeof(HistoricalGlChart), new PropertyMetadata(new AxisOption()));

    public AxisOption AxisYOption
    {
        get { return (AxisOption)GetValue(AxisYOptionProperty); }
        set { SetValue(AxisYOptionProperty, value); }
    }

    public static readonly DependencyProperty IsAutoYAxisEnableProperty = DependencyProperty.Register(
        nameof(IsAutoYAxisEnable), typeof(bool), typeof(HistoricalGlChart), new PropertyMetadata(default(bool)));

    public bool IsAutoYAxisEnable
    {
        get { return (bool)GetValue(IsAutoYAxisEnableProperty); }
        set { SetValue(IsAutoYAxisEnableProperty, value); }
    }

    #endregion

    #region label

    public static readonly DependencyProperty XYMapperProperty = DependencyProperty.Register(
        "XYMapper", typeof(IMapper), typeof(HistoricalGlChart), new PropertyMetadata(default(IMapper)));

    public IMapper XYMapper
    {
        get { return (IMapper)GetValue(XYMapperProperty); }
        set { SetValue(XYMapperProperty, value); }
    }

    public static readonly DependencyProperty LegendVisibilityProperty = DependencyProperty.Register(
        "LegendVisibility", typeof(Visibility), typeof(HistoricalGlChart),
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
        DependencyProperty.Register("LegendTemplate", typeof(DataTemplate), typeof(HistoricalGlChart),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ToolTipTemplateProperty = DependencyProperty.Register(
        "ToolTipTemplate", typeof(DataTemplate), typeof(HistoricalGlChart),
        new PropertyMetadata(default(DataTemplate)));

    public DataTemplate ToolTipTemplate
    {
        get { return (DataTemplate)GetValue(ToolTipTemplateProperty); }
        set { SetValue(ToolTipTemplateProperty, value); }
    }

    public static readonly DependencyProperty RangeSliderAutoToolTipValueTemplateProperty =
        DependencyProperty.Register(
            "RangeSliderAutoToolTipValueTemplate", typeof(DataTemplate), typeof(HistoricalGlChart),
            new PropertyMetadata(default(DataTemplate)));

    public DataTemplate RangeSliderAutoToolTipValueTemplate
    {
        get { return (DataTemplate)GetValue(RangeSliderAutoToolTipValueTemplateProperty); }
        set { SetValue(RangeSliderAutoToolTipValueTemplateProperty, value); }
    }

    public static readonly DependencyProperty RangeSliderAutoToolTipRangeValuesTemplateProperty =
        DependencyProperty.Register(
            "RangeSliderAutoToolTipRangeValuesTemplate", typeof(DataTemplate), typeof(HistoricalGlChart),
            new PropertyMetadata(default(DataTemplate)));

    public DataTemplate RangeSliderAutoToolTipRangeValuesTemplate
    {
        get { return (DataTemplate)GetValue(RangeSliderAutoToolTipRangeValuesTemplateProperty); }
        set { SetValue(RangeSliderAutoToolTipRangeValuesTemplateProperty, value); }
    }

    #endregion

    private ContentRangeSlider _rangeSlider;

    private Series2DChart _contentChart;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _rangeSlider = GetTemplateChild(SliderName) as ContentRangeSlider;
        _rangeSlider.RangeSelectionChanged += RangeSlider_RangeSelectionChanged;
        _contentChart = GetTemplateChild(ContentChart) as CoordinateChart;
        _contentChart.ChangeRegionRequest += _contentChart_ScaleRequest;
        var axisY = BindingSource.AxisY;
        var axisX = BindingSource.AxisX;
        var axisXDisplayRange = axisX.DisplayRange;
        var axisYDisplayRange = axisY.DisplayRange;
        var displayRegion = new CoordinateRegion(axisXDisplayRange, axisYDisplayRange);
        if (displayRegion.IsEmpty())
        {
            displayRegion = new CoordinateRegion(100, 0, 0, 100);
        }

        _contentChart.DisplayRegion = displayRegion;
        _historicalChart = GetTemplateChild(ThumbnailElementName) as CoordinateChartLight;
        _historicalChart.DisplayRegion = displayRegion;
        this.AxisXBoundary = axisX.Boundary;
        this.ScrollBarViewRange = axisXDisplayRange;
        BindingSourcePropertyChangedCallback = BindingSourceChangedCallbackMethod;
        BindingSourceChangedCallbackMethod(this,
            new DependencyPropertyChangedEventArgs(BindingSourceProperty, null, this.BindingSource));
        OnTemplateApplied();
    }

    private void _contentChart_ScaleRequest(CoordinateRegion obj)
    {
        this.InteractMode = ChartInteractMode.Manual;
        this.ScrollBarViewRange = obj.XRange;
    }

    private void RangeSlider_RangeSelectionChanged(object sender,
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
    }

    public void AttachWindow(Window window)
    {
        this._historicalChart.AttachWindow(window);
        this._contentChart.AttachWindow(window);
    }

    public void DetachWindow()
    {
        this._historicalChart.DetachWindow();
        this._contentChart.DetachWindow();
    }

    protected virtual void OnTemplateApplied()
    {
        TemplateApplied?.Invoke(this, EventArgs.Empty);
    }

    public class OpenGlChartAdapter<T> : ChartAdapter<HistoricalGlChart, T>
    {
        private readonly ConcurrentDictionary<BindingEntry<T>, Tuple<ILineChartSeriesItem, ILineChartSeriesItem>>
            _itemsDictionary =
                new ConcurrentDictionary<BindingEntry<T>, Tuple<ILineChartSeriesItem, ILineChartSeriesItem>>();

        private readonly Lazy<bool> _isChildTypeLazy =
            new Lazy<bool>(() => typeof(IPoint).IsAssignableFrom(typeof(T)),
                LazyThreadSafetyMode.ExecutionAndPublication);

        public OpenGlChartAdapter(HistoricalGlChart chartControl, BindingSource<T> source)
            : base(chartControl, source)
        {
            var axisX = source.AxisX;
            var axisY = source.AxisY;
            axisY.PropertyChanged += AxisY_PropertyChanged;
            axisX.PropertyChanged += AxisX_PropertyChanged;
        }

        private void AxisY_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var chartContentChart = Chart._contentChart;
            var sourceAxisY = Source.AxisY;
            switch (e.PropertyName)
            {
                case nameof(AxisOption.IsAutoSize):
                    chartContentChart.AutoYAxis = sourceAxisY.IsAutoSize;
                    break;
                case nameof(AxisOption.DisplayRange):
                    chartContentChart.DisplayRegion =
                        chartContentChart.DisplayRegion.ChangeYRange(sourceAxisY.DisplayRange);
                    break;
                case nameof(AxisOption.ScrollMinRange):
                    chartContentChart.MinYRange = (float)sourceAxisY.ScrollMinRange;
                    break;
            }
        }

        private void AxisX_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var axisX = Source.AxisX;
            switch (e.PropertyName)
            {
                case nameof(AxisOption.DisplayRange):
                    Chart.ScrollBarView = axisX.DisplayRange;
                    break;
                case nameof(AxisOption.ScrollMinRange):
                    Chart._contentChart.MinXRange = (float)axisX.ScrollMinRange;
                    break;
                case nameof(AxisOption.Boundary):
                    var axisXBoundary = axisX.Boundary;
                    switch (Chart.ChartMode)
                    {
                        case ChartType.TimelineTendency:
                            Chart.SetPointsAxisXRange(axisXBoundary);
                            break;
                        case ChartType.Direct:
                            Chart.ReSetByInteractMode(ChartInteractMode.Manual, axisXBoundary);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
            }
        }

        protected override void BindingSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        protected override CombineDisposal AddNewSourceEntryCore(BindingEntry<T> entry)
        {
            var contentChart = this.Chart._contentChart;
            var seriesItem = contentChart.NewSeries();
            var contentDisposable = AddNewPointsSources(seriesItem, entry);
            contentChart.Add(seriesItem);
            if (Chart.ChartMode == ChartType.TimelineTendency) //如果为趋势图会创建一个副本表示历史
            {
                var historicalChart = this.Chart._historicalChart;
                var historyItem = historicalChart.NewSeries();
                var historyDisposable = AddNewPointsSources(historyItem, entry);
                historicalChart.Add(historyItem);
                _itemsDictionary.TryAdd(entry,
                    new Tuple<ILineChartSeriesItem, ILineChartSeriesItem>(seriesItem, historyItem));
                return new CombineDisposal(contentDisposable, historyDisposable);
            }

            _itemsDictionary.TryAdd(entry, new Tuple<ILineChartSeriesItem, ILineChartSeriesItem>(seriesItem, null));
            return new CombineDisposal(contentDisposable);
        }

        private IDisposable AddNewPointsSources(ILineChartSeriesItem seriesItem, BindingEntry<T> bindingEntry)
        {
            var color = bindingEntry.Color;
            seriesItem.LineColor = Color.FromArgb(color.A, color.R, color.G,
                color.B);
            seriesItem.PointCountLimit = Chart.MaxPointLimit;
            seriesItem.Title = bindingEntry.Name;
            seriesItem.Visible = bindingEntry.Visible;
            var mapper = Chart.XYMapper;
            if (mapper == null && !_isChildTypeLazy.Value)
            {
                throw new NotSupportedException($"Can't convert point to interface {nameof(IPoint)}");
            }

            var rawSource = bindingEntry.RawSource;
            var array = rawSource.ToArray();
            //todo:线程安全
            seriesItem.AddGeometries(mapper != null
                ? array.Select(mapper.Map<T, IPoint>).ToArray()
                : array.Cast<IPoint>().ToArray());
            /*if (array.Any())
            {

            }*/
            rawSource.Subscribe(args => OnSubscribe(seriesItem, args, mapper));
            return new SeriesItemBindingEntryBinding(bindingEntry, seriesItem);
        }

        protected override void RemoveSourceEntryCore(BindingEntry<T> sourceEntry)
        {
            if (_itemsDictionary.TryRemove(sourceEntry, out var value))
            {
                this.Chart._contentChart.Remove(value.Item1);
                var item = value.Item2;
                if (item != null)
                {
                    this.Chart._historicalChart.Remove(item);
                }
            }
        }

        protected override void ClearBindingSourceEntries()
        {
            base.ClearBindingSourceEntries();
            this._itemsDictionary.Clear();
            this.Chart._contentChart.Clear();
            this.Chart._historicalChart.Clear();
        }


        private void OnSubscribe(ILineChartSeriesItem seriesItem, NotifyCollectionChangedEventArgs<T> args,
            IMapper mapper)
        {
            var changedAction = args.Action;
            var argsNewItems = args.NewItems;
            switch (changedAction)
            {
                case NotifyCollectionChangedAction.Add:
                    seriesItem.AddGeometries(mapper != null
                        ? argsNewItems.Select(mapper.Map<T, IPoint>).ToArray()
                        : argsNewItems.Cast<IPoint>().ToArray());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (argsNewItems == null)
                    {
                        seriesItem.Clear();
                    }
                    else
                    {
                        seriesItem.ResetWith(mapper != null
                            ? argsNewItems.Select(mapper.Map<T, IPoint>).ToArray()
                            : argsNewItems.Cast<IPoint>().ToArray());
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Dispose()
        {
            Source.AxisX.PropertyChanged -= AxisX_PropertyChanged;
            Source.AxisY.PropertyChanged -= AxisY_PropertyChanged;
            base.Dispose();
        }

        public class SeriesItemBindingEntryBinding : IDisposable
        {
            private readonly BindingEntry<T> _entry;
            private readonly ILineChartSeriesItem _item;

            private const string VisibleName = nameof(BindingEntry<int>.Visible);

            private const string ColorName = nameof(BindingEntry<int>.Color);

            public SeriesItemBindingEntryBinding(BindingEntry<T> entry, ILineChartSeriesItem item)
            {
                this._entry = entry;
                this._item = item;
                entry.PropertyChanged += Entry_PropertyChanged;
            }

            private void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case VisibleName:
                        _item.Visible = _entry.Visible;
                        break;
                    case ColorName:
                        var entryColor = _entry.Color;
                        _item.LineColor = Color.FromArgb(entryColor.R, entryColor.G, entryColor.B);
                        break;
                }
            }

            public void Dispose()
            {
                _entry.PropertyChanged -= Entry_PropertyChanged;
            }
        }
    }
}