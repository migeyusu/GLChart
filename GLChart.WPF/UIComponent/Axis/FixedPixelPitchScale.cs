using System;

namespace GLChart.WPF.UIComponent.Axis
{
    /// <summary>
    /// 固定的像素间距，表现为无论图放大缩小在界面上都是固定间隔
    /// </summary>
    public class FixedPixelPitchScale : CountBasedScaleGenerator
    {
        /// <summary>
        /// pitch of scale,it's a virtual value, maybe any of measurement
        /// </summary>
        public double Pitch { get; }

        protected override int GetScaleCount(ScaleLineGenerationContext context)
        {
            return (int)Math.Floor(context.PixelStretch / this.Pitch);
        }

        public FixedPixelPitchScale(double pitch)
        {
            Pitch = pitch;
        }
    }
}