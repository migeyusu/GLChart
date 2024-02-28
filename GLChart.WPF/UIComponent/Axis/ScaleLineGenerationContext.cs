using System.Windows;
using GLChart.WPF.Base;
using GLChart.WPF.Render;

namespace GLChart.WPF.UIComponent.Axis
{
    /// <summary>
    /// context used to generate scale 
    /// </summary>
    public sealed class ScaleLineGenerationContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueRange"></param>
        /// <param name="pixelStart"></param>
        /// <param name="pixelStretch"></param>
        /// <param name="pixelDirection">if value is <see cref="FlowDirection.LeftToRight"/>,means pixel is growth,another inverse</param>
        /// <param name="axisRenderOption"></param>
        public ScaleLineGenerationContext(ScrollRange valueRange, double pixelStart, double pixelStretch,
            FlowDirection pixelDirection, AxisRenderOption axisRenderOption)
        {
            ValueRange = valueRange;
            PixelStart = pixelStart;
            PixelStretch = pixelStretch;
            PixelDirection = pixelDirection;
            AxisRenderOption = axisRenderOption;
        }

        public AxisRenderOption AxisRenderOption { get; }

        public ScrollRange ValueRange { get; }

        public double PixelStart { get; }

        public double PixelStretch { get; }

        public FlowDirection PixelDirection { get; }
    }
}