using System;

namespace GLChart.WPF.UIComponent.Axis;

public class MarginFluentPixelPitchScale : CountBasedScaleGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pixelStages"></param>
    public MarginFluentPixelPitchScale(double[] pixelStages) : base()
    {
        PixelStages = pixelStages;
    }

    public MarginFluentPixelPitchScale() : this(new double[] { 100, 200, 400 })
    {
    }
    
    public double[] PixelStages { get; }

    public const int MinCount = 2;

    protected override int GetScaleCount(ScaleLineGenerationContext context)
    {
        var pixelStretch = context.PixelStretch;
        var findIndex = Array.FindIndex(PixelStages, d => d >= pixelStretch);
        if (findIndex < 0)
        {
            return findIndex + PixelStages.Length;
        }

        return findIndex + MinCount;
    }
}