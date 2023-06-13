using System.Collections.Generic;

namespace GLChart.Interface.Abstraction
{
    /// <summary>
    /// 刻度生成器
    /// </summary>
    public interface IScaleGenerator
    {
        IEnumerable<AxisScale> Generate(ScaleGenerationContext context);
    }
}