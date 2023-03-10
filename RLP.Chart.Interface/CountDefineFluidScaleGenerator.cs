using System;
using System.Collections.Generic;
using System.Windows;
using RLP.Chart.Interface.Abstraction;

namespace RLP.Chart.Interface
{
    /// <summary>
    /// 利用子类提供的线条数量的刻度生成器
    /// </summary>
    public abstract class CountDefineFluidScaleGenerator : IScaleGenerator
    {
        /// <summary>
        /// 如果范围小于该值，从<see cref="ValueStart"/>开始产生刻度
        /// </summary>
        public double ValueStart { get; }

        protected CountDefineFluidScaleGenerator(double valueStart)
        {
            ValueStart = valueStart;
        }

        protected CountDefineFluidScaleGenerator() : this(0)
        {
        }

        protected abstract int GetScaleCount(ScaleGenerationContext context);

        public IEnumerable<AxisScale> Generate(ScaleGenerationContext context)
        {
            var valueRange = context.ValueRange;
            var pixelStretch = context.PixelStretch;
            var valueRangeStart = valueRange.Start;
            var pixelStart = context.PixelStart;
            var pixelDirection = context.PixelDirection;
            double pixelStartOffset;
            if (ValueStart <= valueRangeStart)
            {
                pixelStartOffset = 0; //像素度量的起始偏移量为0，表示以像素起点为真实起点
            }
            else
            {
                pixelStartOffset = (ValueStart - valueRangeStart) / valueRange.Range * pixelStretch;
                if (context.AxisRenderOption.TextHeight < Math.Abs(pixelStartOffset) + 2)
                {
                    //存在明显偏移（超过偏移量）时给予一个额外的起始位刻度
                    yield return new AxisScale() { Location = (float)pixelStart, Value = valueRangeStart };
                }

                valueRangeStart = ValueStart;
            }

            pixelStretch -= pixelStartOffset;
            pixelStart = pixelDirection == FlowDirection.LeftToRight
                ? pixelStart + pixelStartOffset
                : pixelStart - pixelStartOffset;
            var count = GetScaleCount(new ScaleGenerationContext(valueRange, pixelStart, pixelStretch,
                pixelDirection, context.AxisRenderOption)); //创建一个虚拟的轴刻度
            var pixelStep = pixelStretch / count;
            if (pixelDirection != FlowDirection.LeftToRight)
            {
                pixelStep = -pixelStep;
            }

            var valueStep = valueRange.Range / count;
            for (var i = 0; i < count; i++)
            {
                var position = (float)(pixelStart + pixelStep * i);
                yield return new AxisScale() { Location = position, Value = valueRangeStart + valueStep * i };
            }
        }
    }
}