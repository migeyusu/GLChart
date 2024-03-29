﻿using System;

namespace GLChart.WPF.UIComponent.Axis
{
    public class FixedPixelPitchScale : CountBasedScaleGenerator
    {
        private readonly int _startPixel;
        private readonly int _startCount;
        private readonly int _stepPixel;
        private readonly int _stepCount;

        public FixedPixelPitchScale(int startPixel, int startCount, int stepPixel,
            int stepCount) : base()
        {
            this._startPixel = startPixel;
            this._startCount = startCount;
            this._stepPixel = stepPixel;
            this._stepCount = stepCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPixel">可以产生分划线的最小像素长度</param>
        /// <param name="stepPixel">像素间隔</param>
        public FixedPixelPitchScale(int startPixel, int stepPixel) :
            this(startPixel, 2, stepPixel, 1)
        {
        }


        protected override int GetScaleCount(ScaleLineGenerationContext context)
        {
            var pixelStretch = context.PixelStretch;
            if (pixelStretch <= _startPixel)
            {
                return _startCount;
            }

            return (int)Math.Floor((pixelStretch - _startPixel) / _stepPixel * _stepCount) + _startCount;
        }
    }
}