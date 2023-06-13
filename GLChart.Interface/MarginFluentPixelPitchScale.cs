using System;

namespace RLP.Chart.Interface;

public class MarginFluentPixelPitchScale : CountBasedScaleGenerator
{
    /// <summary>
    /// 从<paramref name="valueStart"/>值的起点
    /// </summary>
    /// <param name="valueStart"></param>
    /// <param name="pixelStages"></param>
    public MarginFluentPixelPitchScale(double valueStart, double[] pixelStages) : base(valueStart)
    {
        PixelStages = pixelStages;
    }

    public MarginFluentPixelPitchScale() : this(0d, new double[] { 100, 200, 400 })
    {
    }


    public double[] PixelStages { get; }

    public const int MinCount = 2;

    protected override int GetScaleCount(ScaleGenerationContext context)
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