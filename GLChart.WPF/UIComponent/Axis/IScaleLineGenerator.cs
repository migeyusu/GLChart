using System.Collections.Generic;

namespace GLChart.WPF.UIComponent.Axis
{
    /// <summary>
    /// 刻度生成器
    /// </summary>
    public interface IScaleLineGenerator
    {
        IEnumerable<AxisScale> Generate(ScaleLineGenerationContext context);
    }
}