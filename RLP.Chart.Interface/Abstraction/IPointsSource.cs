using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace RLP.Chart.Interface.Abstraction
{
    /// <summary>
    /// 可绑定的源，允许延迟订阅（实现者应实现缓存机制）
    /// </summary>
    public interface IPointsSource<T> : IEnumerable<T>, IDisposable
    {
        /// <summary>
        /// [thread safety] 订阅源
        /// </summary>
        /// <param name="action"></param>
        /// <returns>取消订阅</returns>
        IDisposable Subscribe(Action<NotifyCollectionChangedEventArgs<T>> action);

        void Append(T point);

        void AppendRange(IEnumerable<T> points);

        /// <summary>
        /// 使用指定的点位重置
        /// </summary>
        /// <param name="points"></param>
        void ResetWith(IEnumerable<T> points);

        /// <summary>
        /// 清空
        /// </summary>
        void Clear();
        void First();

        void Last();
    }
}