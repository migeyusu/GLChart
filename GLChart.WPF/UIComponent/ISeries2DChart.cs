using System.Collections.Generic;
using GLChart.WPF.Base;

namespace GLChart.WPF.UIComponent;

/// <summary>
/// 可渲染系列的图表,一个系列表示可绘制的一类几何体。
/// <para>没有类型限制，允许系列表持有多类型集合</para>
/// </summary>
public interface ISeries2DChart
{
    /// <summary>
    /// 已附加的系列
    /// </summary>
    IReadOnlyList<ISeries2D> SeriesItems { get; }

    /// <summary>
    /// 创建一个新的系列
    /// </summary>
    /// <returns></returns>
    T NewSeries<T>() where T : ISeries2D;

    /// <summary>
    /// 移除系列    
    /// </summary>
    /// <param name="seriesItem"></param>
    void Remove(ISeries2D seriesItem);

    /// <summary>
    /// 清空所有系列 
    /// </summary>
    void Clear();
}