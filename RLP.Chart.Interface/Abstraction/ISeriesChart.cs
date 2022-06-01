using System.Collections.Generic;

namespace RLP.Chart.Interface.Abstraction
{
    /// <summary>
    /// 系列构成的图表
    /// </summary>
    public interface ISeriesChart<T> where T : ISeriesItem
    {
        /// <summary>
        /// 已附加的系列
        /// </summary>
        IReadOnlyList<T> SeriesItems { get; }

        /// <summary>
        /// 创建一个新的系列项
        /// </summary>
        /// <returns></returns>
        T NewSeries();

        void Add(T item);

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