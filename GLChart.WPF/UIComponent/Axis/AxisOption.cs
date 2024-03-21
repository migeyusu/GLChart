using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;

namespace GLChart.WPF.UIComponent.Axis
{
    public abstract class AxisOption : FrameworkElement
    {
        static AxisOption()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AxisOption),
                new FrameworkPropertyMetadata(typeof(AxisOption)));
        }

        #region user custom

        public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register(
            nameof(IsSeparatorVisible), typeof(bool), typeof(AxisOption),
            new FrameworkPropertyMetadata(true));

        public bool IsSeparatorVisible
        {
            get { return (bool)GetValue(IsSeparatorVisibleProperty); }
            set { SetValue(IsSeparatorVisibleProperty, value); }
        }

        public static readonly DependencyProperty SeparatorPenProperty = DependencyProperty.Register(
            nameof(SeparatorPen), typeof(Pen), typeof(AxisOption), new FrameworkPropertyMetadata(
                new Pen(Brushes.Gray, 0.5d)
                    { DashStyle = new DashStyle(new double[] { 10, 10, 0, 10 },10) }));

        public Pen SeparatorPen
        {
            get { return (Pen)GetValue(SeparatorPenProperty); }
            set { SetValue(SeparatorPenProperty, value); }
        }

        public static readonly DependencyProperty SeparatorZeroPenProperty = DependencyProperty.Register(
            nameof(SeparatorZeroPen), typeof(Pen), typeof(AxisOption), new PropertyMetadata(
                new Pen(Brushes.DimGray, 2)
                {
                    DashStyle = DashStyles.Solid,
                }));

        public Pen SeparatorZeroPen
        {
            get { return (Pen)GetValue(SeparatorZeroPenProperty); }
            set { SetValue(SeparatorZeroPenProperty, value); }
        }

        /// <summary>
        /// 刻度标签
        /// </summary>
        public static readonly DependencyProperty AxisLabelsProperty = DependencyProperty.Register(
            nameof(AxisLabels), typeof(IEnumerable<AxisLabel>), typeof(AxisOption),
            new FrameworkPropertyMetadata(
                default(IEnumerable<AxisLabel>), FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 刻度标签
        /// </summary>
        public IEnumerable<AxisLabel> AxisLabels
        {
            get { return (IEnumerable<AxisLabel>)GetValue(AxisLabelsProperty); }
            set { SetValue(AxisLabelsProperty, value); }
        }

        /// <summary>
        /// 标签convert函数
        /// </summary>
        public static readonly DependencyProperty ScaleLabelFuncProperty = DependencyProperty.Register(
            nameof(ScaleLabelFunc), typeof(Func<double, string>), typeof(AxisOption),
            typeMetadata: new PropertyMetadata(new Func<double, string>(f =>
                ((float)f).ToString(CultureInfo.InvariantCulture)), RefreshLabelsPropertyChangedCallback));

        protected static void RefreshLabelsPropertyChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is AxisOption option)
            {
                option.RefreshLabels(option.RenderSize);
            }
        }

        /// <summary>
        /// 标签convert函数
        /// </summary>
        public Func<double, string> ScaleLabelFunc
        {
            get { return (Func<double, string>)GetValue(ScaleLabelFuncProperty); }
            set { SetValue(ScaleLabelFuncProperty, value); }
        }

        /// <summary>
        /// 刻度生成器
        /// </summary>
        public IScaleLineGenerator ScaleGenerator { get; set; }
            = new FixedPixelPitchScale(50, 100);

        public static readonly DependencyProperty LabelRenderOptionProperty = DependencyProperty.Register(
            nameof(LabelRenderOption), typeof(AxisLabelRenderOption), typeof(AxisOption),
            new FrameworkPropertyMetadata(new AxisLabelRenderOption(), RefreshLabelsPropertyChangedCallback));

        public AxisLabelRenderOption LabelRenderOption
        {
            get { return (AxisLabelRenderOption)GetValue(LabelRenderOptionProperty); }
            set { SetValue(LabelRenderOptionProperty, value); }
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

        public static readonly DependencyProperty ViewRangeProperty = DependencyProperty.Register(
            nameof(ViewRange), typeof(ScrollRange), typeof(AxisOption),
            new PropertyMetadata(default(ScrollRange), RefreshLabelsPropertyChangedCallback));

        /// <summary>
        /// 当前视图区域
        /// </summary>
        public ScrollRange ViewRange
        {
            get { return (ScrollRange)GetValue(ViewRangeProperty); }
            set { SetValue(ViewRangeProperty, value); }
        }

        #endregion

        /// <summary>
        /// 对视图区域执行偏移
        /// </summary>
        /// <param name="newRange">新视图区域</param>
        public void TryMoveView(ScrollRange newRange)
        {
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

            this.ViewRange = newRange;
        }

        public void TryScaleView(ScrollRange newRange)
        {
            if (newRange.Range < this.MinDisplayExtent)
            {
                return;
            }

            var boundary = this.ZoomBoundary;
            if (boundary != null)
            {
                var end = boundary.Value.End;
                var start = boundary.Value.Start;
                end = newRange.End > end ? end : newRange.End;
                start = newRange.Start < start ? start : newRange.Start;
                newRange = new ScrollRange(start, end);
            }

            this.ViewRange = newRange;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            RenderScale(drawingContext);
        }

        /// <summary>
        /// 渲染坐标刻度
        /// </summary>
        /// <param name="context"></param>
        protected abstract void RenderScale(DrawingContext context);

        /// <summary>
        /// create labels collection on a axis
        /// 返回指定的标签
        /// </summary>
        protected abstract void RefreshLabels(Size size);

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            RefreshLabels(sizeInfo.NewSize);
        }
    }
}