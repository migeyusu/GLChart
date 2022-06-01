using System;

namespace RLP.Chart.Interface
{
    /// <summary>
    /// 固定值间距，表现为放大时间距变大，增大range时刻度变多
    /// </summary>
    public class FixedValuePitchScale : CountDefineFluidScaleGenerator
    {
        /// <summary>
        /// pitch of scale,it's a virtual value, maybe any of measurement
        /// </summary>
        public double Pitch { get; }

        public FixedValuePitchScale(double pitch)
        {
            Pitch = pitch;
        }

        protected override int GetScaleCount(ScaleGenerationContext context)
        {
            return (int)Math.Floor(context.ValueRange.Range / this.Pitch);
        }
    }
}