﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using GLChart.WPF.Base;

namespace GLChart.WPF.UIComponent.Axis
{
    /// <summary>
    /// 利用子类提供的线条数量的刻度生成器
    /// </summary>
    public abstract class CountBasedScaleGenerator : IScaleLineGenerator
    {
        /// <summary>
        /// 刻度是否可以流动。是：优先绑定到10的次方数
        /// </summary>
        public bool IsFluent { get; set; } = true;

        protected CountBasedScaleGenerator()
        {
        }

        protected abstract int GetScaleCount(ScaleLineGenerationContext context);

        public IEnumerable<AxisScaleLine> Generate(ScaleLineGenerationContext context)
        {
            //获取分划数
            var count = GetScaleCount(context);
            var valueRange = context.ValueRange;
            var valueRangeStart = valueRange.Start;
            var valueRangeRange = valueRange.Range;
            var pixelStretch = context.PixelStretch;
            double pixelStartOffset;
            double pixelStep;
            double firstSpan;
            double span;
            if (IsFluent)
            {
                span = GetStickSpan(valueRangeRange, count);
                var rangeStart = valueRangeStart / span;
                var floor = Math.Floor(rangeStart);
                if (!floor.AlmostSame(rangeStart))
                {
                    floor++;
                }

                firstSpan = (int)floor * span;
                pixelStartOffset = (firstSpan - valueRangeStart) / valueRangeRange * pixelStretch;
                pixelStep = span / valueRangeRange * pixelStretch;
                count = (int)Math.Floor((pixelStretch - pixelStartOffset) / pixelStep);
            }
            else
            {
                pixelStartOffset = 0;
                pixelStretch -= pixelStartOffset;
                pixelStep = pixelStretch / count;
                span = valueRange.Range / count;
                firstSpan = valueRangeStart;
            }

            if (context.PixelDirection != FlowDirection.LeftToRight)
            {
                pixelStartOffset = -pixelStartOffset;
                pixelStep = -pixelStep;
            }

            var pixelStart = context.PixelStart + pixelStartOffset;
            for (var i = 0; i <= count; i++)
            {
                var position = (float)(pixelStart + pixelStep * i);
                yield return new AxisScaleLine { Location = position, Value = firstSpan + span * i };
            }
        }

        private static double GetStickSpan(double value, int spansCount)
        {
            var round = Math.Round(value);
            int span = (int)Math.Floor(round / spansCount);
            int digit = 0;
            while (span > 10)
            {
                span /= 10;
                digit++;
            }

            return span * Math.Pow(10, digit);
        }
    }
}