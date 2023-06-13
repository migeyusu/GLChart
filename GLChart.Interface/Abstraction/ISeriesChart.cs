using System.Collections.Generic;

namespace GLChart.Interface.Abstraction
{
    /// <summary>
    /// 可渲染系列的图表,一个系列表示可绘制的一类几何体。
    /// <para>没有类型限制，允许系列表持有多类型集合</para>
    /// </summary>
    public interface ISeriesChart<T>
    {
        /// <summary>
        /// 已附加的系列
        /// </summary>
        IReadOnlyList<T> SeriesItems { get; }

        /// <summary>
        /// 创建一个新的系列
        /// </summary>
        /// <returns></returns>
        T NewSeries();

        /// <summary>
        /// 移除系列
        /// </summary>
        /// <param name="seriesItem"></param>
        void Remove(T seriesItem);

        /// <summary>
        /// 清空所有系列 
        /// </summary>
        void Clear();
    }
}