using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;

namespace GLChart.WPF.UIComponent.Axis
{
    public class AxisXOption : AxisOption
    {
        protected override FlowDirection NumberDirection { get; } = FlowDirection.LeftToRight;
    }

    public class AxisYOption : AxisOption
    {
        /// <summary>
        /// 是否自适应高度
        /// </summary>
        public static readonly DependencyProperty IsAutoSizeProperty = DependencyProperty.Register(
            nameof(IsAutoSize), typeof(bool), typeof(AxisYOption), new PropertyMetadata(default(bool)));

        /// <summary>
        /// 是否自适应高度
        /// </summary>
        public bool IsAutoSize
        {
            get { return (bool)GetValue(IsAutoSizeProperty); }
            set { SetValue(IsAutoSizeProperty, value); }
        }

        protected override FlowDirection NumberDirection { get; } = FlowDirection.RightToLeft;
    }

    public abstract class AxisOption : DependencyObject
    {
        #region user custom

        public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register(
            nameof(IsSeparatorVisible), typeof(bool), typeof(AxisOption),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsSeparatorVisible
        {
            get { return (bool)GetValue(IsSeparatorVisibleProperty); }
            set { SetValue(IsSeparatorVisibleProperty, value); }
        }

        public static readonly DependencyProperty SeparatorPenProperty = DependencyProperty.Register(
            nameof(SeparatorPen), typeof(Pen), typeof(AxisOption), new FrameworkPropertyMetadata(
                new Pen(Brushes.Gray, 0.5d)
                    { DashStyle = DashStyles.DashDotDot }, FrameworkPropertyMetadataOptions.AffectsRender));

        public Pen SeparatorPen
        {
            get { return (Pen)GetValue(SeparatorPenProperty); }
            set { SetValue(SeparatorPenProperty, value); }
        }

        /// <summary>
        /// 标签convert函数
        /// </summary>
        public Func<double, string> ScaleLabelFunc { get; set; } =
            f => ((float)f).ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// 刻度生成器
        /// </summary>
        public IScaleLineGenerator ScaleGenerator { get; set; }
            = new FixedPixelPitchScale(50, 100);

        public static readonly DependencyProperty RenderOptionProperty = DependencyProperty.Register(
            nameof(RenderOption), typeof(AxisLabelRenderOption), typeof(AxisOption),
            new FrameworkPropertyMetadata(new AxisLabelRenderOption(), FrameworkPropertyMetadataOptions.AffectsRender));

        public AxisLabelRenderOption RenderOption
        {
            get { return (AxisLabelRenderOption)GetValue(RenderOptionProperty); }
            set { SetValue(RenderOptionProperty, value); }
        }

        /// <summary>
        /// 数轴方向
        /// </summary>
        protected abstract FlowDirection NumberDirection { get; }

        /// <summary>
        /// 保留位数
        /// </summary>
        public int RoundDigit { get; set; } = 2;


        /// <summary>
        /// 最小缩放范围
        /// </summary>
        public static readonly DependencyProperty MinDisplayExtentProperty = DependencyProperty.Register(
            nameof(MinDisplayExtent), typeof(double), typeof(AxisOption), new PropertyMetadata(1d));

        /// <summary>
        /// 最小缩放范围
        /// </summary>
        public double MinDisplayExtent
        {
            get { return (double)GetValue(MinDisplayExtentProperty); }
            set { SetValue(MinDisplayExtentProperty, value); }
        }

        /// <summary>
        /// 是否允许缩放
        /// </summary>
        public static readonly DependencyProperty ZoomEnableProperty = DependencyProperty.Register(
            nameof(ZoomEnable), typeof(bool), typeof(AxisOption), new PropertyMetadata(true));

        /// <summary>
        /// 是否允许缩放
        /// </summary>
        public bool ZoomEnable
        {
            get { return (bool)GetValue(ZoomEnableProperty); }
            set { SetValue(ZoomEnableProperty, value); }
        }

        public static readonly DependencyProperty ZoomBoundaryProperty = DependencyProperty.Register(
            nameof(ZoomBoundary), typeof(ScrollRange?), typeof(AxisOption),
            new FrameworkPropertyMetadata(new ScrollRange(0, 100)));

        /// <summary>
        /// 轴缩放边界，超过该边界将重置
        /// </summary>
        public ScrollRange? ZoomBoundary
        {
            get { return (ScrollRange?)GetValue(ZoomBoundaryProperty); }
            set { SetValue(ZoomBoundaryProperty, value); }
        }

        public static readonly DependencyProperty CurrentViewRangeProperty = DependencyProperty.Register(
            nameof(CurrentViewRange), typeof(ScrollRange), typeof(AxisOption),
            new PropertyMetadata(default(ScrollRange)));

        /// <summary>
        /// 当前视图区域
        /// </summary>
        public ScrollRange CurrentViewRange
        {
            get { return (ScrollRange)GetValue(CurrentViewRangeProperty); }
            set { SetValue(CurrentViewRangeProperty, value); }
        }

        #endregion

        private IList<AxisLabel>? _cacheLabels;

        private double _lastPixelStart, _lastPixelStretch;

        /// <summary>
        /// 对视图区域执行偏移
        /// </summary>
        /// <param name="newRange">新视图区域</param>
        public void TryMoveView(ScrollRange newRange)
        {
            if (newRange.Range < this.MinDisplayExtent)
            {
                return;
            }

            var xZoomBoundary = this.ZoomBoundary;
            if (xZoomBoundary != null)
            {
                var right = xZoomBoundary.Value.End;
                var start = xZoomBoundary.Value.Start;
                var xExtend = newRange.Range;
                if (newRange.End > right)
                {
                    var xStart = right - xExtend;
                    newRange = new ScrollRange(xStart, right);
                }
                else if (newRange.Start < start)
                {
                    var xEnd = start + xExtend;
                    newRange = new ScrollRange(start, xEnd);
                }
            }

            this.CurrentViewRange = newRange;
        }

        /// <summary>
        /// create labels collection on a axis
        /// 返回指定的标签
        /// </summary>
        /// <param name="pixelStart">像素起始点</param>
        /// <param name="pixelStretch">像素长度</param>
        /// <returns></returns>
        public IList<AxisLabel> GenerateLabels(double pixelStart, double pixelStretch)
        {
            if (_lastPixelStart.Equals(pixelStart) && _lastPixelStretch.Equals(pixelStretch)
                                                   && _cacheLabels != null)
            {
                return _cacheLabels;
            }

            _lastPixelStart = pixelStart;
            _lastPixelStretch = pixelStretch;
            var labelFunc = this.ScaleLabelFunc;
            _cacheLabels = this.ScaleGenerator
                .Generate(new ScaleLineGenerationContext(this.CurrentViewRange, pixelStart, pixelStretch,
                    this.NumberDirection,
                    this.RenderOption))
                .Select(scale =>
                {
                    var round = Math.Round(scale.Value, this.RoundDigit);
                    return new AxisLabel()
                    {
                        Location = scale.Location,
                        Value = round,
                        Text = labelFunc.Invoke(round),
                    };
                }).ToList();
            return _cacheLabels;
        }
    }
}