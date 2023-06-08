using System;

namespace RLP.Chart.Interface
{
    public class MarginSteppedFluentPixelPitchScale : CountBasedScaleGenerator
    {
        private readonly int _startPixel;
        private readonly int _startCount;
        private readonly int _stepPixel;
        private readonly int _stepCount;

        public MarginSteppedFluentPixelPitchScale(double valueStart, int startPixel, int startCount, int stepPixel,
            int stepCount) : base(valueStart)
        {
            this._startPixel = startPixel;
            _startCount = startCount;
            this._stepPixel = stepPixel;
            _stepCount = stepCount;
        }

        public MarginSteppedFluentPixelPitchScale(double valueStart, int startPixel, int stepPixel) :
            this(valueStart, startPixel, 2, stepPixel, 1)
        {
        }


        protected override int GetScaleCount(ScaleGenerationContext context)
        {
            var pixelStretch = context.PixelStretch;
            if (pixelStretch <= _startPixel)
            {
                return _startCount;
            }

            return (int)Math.Floor(((pixelStretch - _startPixel) / _stepPixel) * _stepCount) + _startCount;
        }
    }
}