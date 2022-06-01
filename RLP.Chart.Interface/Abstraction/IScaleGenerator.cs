using System.Collections.Generic;

namespace RLP.Chart.Interface.Abstraction
{
    /// <summary>
    /// 刻度生成器
    /// </summary>
    public interface IScaleGenerator
    {
        IEnumerable<AxisScale> Generate(ScaleGenerationContext context);
    }
}