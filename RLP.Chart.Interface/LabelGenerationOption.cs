using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RLP.Chart.Interface.Abstraction;

namespace RLP.Chart.Interface
{
    public class LabelGenerationOption
    {
        public static LabelGenerationOption Default
        {
            get
            {
                return new LabelGenerationOption()
                {
                    LabelFunc = f => ((float)f).ToString(),
                    RenderOption = AxisRenderOption.Default(),
                    RoundDigit = 2,
                };
            }
        }

        /// <summary>
        /// 标签convert函数
        /// </summary>
        public Func<double, string> LabelFunc { get; set; }

        public IScaleGenerator ScaleGenerator { get; set; }

        public AxisRenderOption RenderOption { get; set; }

        public FlowDirection FlowDirection { get; set; }

        /// <summary>
        /// 保留位数
        /// </summary>
        public int RoundDigit { get; set; }

        private IList<AxisLabel> _cacheLabels;

        private double _pixelStart, _pixelStretch;
        private ScrollRange _scrollRange;

        /// <summary>
        /// create labels collection on a axis
        /// 返回指定的标签
        /// </summary>
        /// <param name="pixelStart"></param>
        /// <param name="pixelStretch"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public IList<AxisLabel> GenerateLabels(double pixelStart,
            double pixelStretch, ScrollRange range)
        {
            if (_pixelStart.Equals(pixelStart) && _pixelStretch.Equals(pixelStretch) && _scrollRange.Equals(range)
                && _cacheLabels != null)
            {
                return _cacheLabels;
            }

            _pixelStart = pixelStart;
            _pixelStretch = pixelStretch;
            _scrollRange = range;
            var labelFunc = this.LabelFunc;
            _cacheLabels = this.ScaleGenerator
                .Generate(new ScaleGenerationContext(range, pixelStart, pixelStretch, this.FlowDirection,this.RenderOption))
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