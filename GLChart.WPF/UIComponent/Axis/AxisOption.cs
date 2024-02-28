using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render;

namespace GLChart.WPF.UIComponent.Axis
{
    public class AxisOption
    {
        #region user custom

        public bool IsSeparatorVisible { get; set; } = true;

        public Pen SeparatorPen { get; set; } = new Pen(Brushes.Gray, 0.5d)
            { DashStyle = DashStyles.DashDotDot };

        /// <summary>
        /// 标签convert函数
        /// </summary>
        public Func<double, string> ScaleLabelFunc { get; set; } =
            f => ((float)f).ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// 刻度生成器
        /// </summary>
        public IScaleLineGenerator ScaleGenerator { get; set; }
            = new MarginSteppedFluentPixelPitchScale(50, 100);

        public AxisRenderOption RenderOption { get; set; } = AxisRenderOption.Default();

        /// <summary>
        /// 保留位数
        /// </summary>
        public int RoundDigit { get; set; } = 2;

        /// <summary>
        /// 最小缩放范围
        /// </summary>
        public float MinDisplayExtent { get; set; } = 1;

        /// <summary>
        /// 允许缩放
        /// </summary>
        public bool ZoomEnable { get; set; } = true;

        /// <summary>
        /// 轴缩放边界，超过该边界将重置
        /// </summary>
        public ScrollRange ZoomBoundary { get; set; } = new ScrollRange(0, 100);

        #endregion

        private IList<AxisLabel> _cacheLabels;

        private double _lastPixelStart, _lastPixelStretch;

        private ScrollRange _scrollRange;


        /// <summary>
        /// create labels collection on a axis
        /// 返回指定的标签
        /// </summary>
        /// <param name="pixelStart">像素起始点</param>
        /// <param name="pixelStretch">像素长度</param>
        /// <param name="range">数轴范围</param>
        /// <param name="flowDirection">数轴方向</param>
        /// <returns></returns>
        public IList<AxisLabel> GenerateLabels(double pixelStart, double pixelStretch, 
            ScrollRange range, FlowDirection flowDirection)
        {
            if (_lastPixelStart.Equals(pixelStart) && _lastPixelStretch.Equals(pixelStretch)
                                               && _scrollRange.Equals(range) && _cacheLabels != null)
            {
                return _cacheLabels;
            }

            _lastPixelStart = pixelStart;
            _lastPixelStretch = pixelStretch;
            _scrollRange = range;
            var labelFunc = this.ScaleLabelFunc;
            _cacheLabels = this.ScaleGenerator
                .Generate(new ScaleLineGenerationContext(range, pixelStart, pixelStretch, flowDirection,
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