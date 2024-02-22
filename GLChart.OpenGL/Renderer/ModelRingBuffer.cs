using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using GLChart.Core.Collection;
using GLChart.Interface;

namespace GLChart.Core.Renderer
{
    /// <summary>
    /// 模型环形缓冲，允许存储指定的数据模型到环形缓冲。
    /// <see cref="T"/>:模型类型；<see cref="TK"/>：数据类型
    /// </summary>
    public class ModelRingBuffer<T, TK> where TK : struct
    {
        /// <summary>
        /// 模型到比特数组的映射
        /// </summary>
        private Func<T, TK[]> _modelToFloatsMapping;

        /// <summary>
        /// 有效填充区域，第一段为索引更大的部分
        /// </summary>
        public IList<RingBufferCounter.Region> EffectRegions
        {
            get => _ringBufferCounter.ContiguousRegions.ToArray();
        }

        /// <summary>
        /// 最大实体数量
        /// </summary>
        private uint _maxModelCount;

        /// <summary>
        /// 建议的最大模型数，包含了副本节点
        /// </summary>
        public uint SuggestMaxModelCount => _maxModelCount + 1;

        /// <summary>
        /// 模型大小
        /// </summary>
        private uint _modelSize;

        /// <summary>
        /// 当前模型数量
        /// </summary>
        public uint RecentModelCount { get; set; }

        /// <summary>
        /// gpu分配的缓冲区大小
        /// </summary>
        public long DeviceBufferSize { get; set; }

        private RingBufferCounter _ringBufferCounter;

        private readonly ConcurrentQueue<NotifyCollectionChangedEventArgs<T>> _changedEventArgsQueue =
            new ConcurrentQueue<NotifyCollectionChangedEventArgs<T>>();

        /// <summary>
        /// 分配空间
        /// </summary>
        /// <param name="modelSize">模型导出数组的大小，以<see cref="TK"/>为单位</param>
        /// <param name="modelToFloatsMapping">模型导出数组的函数</param>
        /// <param name="maxModelCount">最大模型数量</param>
        public ModelRingBuffer()
        {
        }

        public void Initialize(uint maxModelCount, uint modelSize, Func<T, TK[]> modelToFloatsMapping)
        {
            _modelToFloatsMapping = modelToFloatsMapping;
            _modelSize = modelSize;
            this._maxModelCount = maxModelCount;
            //为了使得环形缓冲的分块绘制点位不断，在gpu缓冲中必须加首部副本节点
            var deviceContainsCount = maxModelCount + 1;
            DeviceBufferSize = deviceContainsCount * modelSize;
            this._ringBufferCounter = new RingBufferCounter((int)(maxModelCount * modelSize));
        }

        public void SendChange(NotifyCollectionChangedEventArgs<T> args)
        {
            _changedEventArgsQueue.Enqueue(args);
        }

        public bool TryFlush(out IList<GPUBufferRegion<TK>> regions)
        {
            if (_changedEventArgsQueue.IsEmpty)
            {
                regions = null;
                return false;
            }

            _changedEventArgsQueue.TryDequeue(out var result);
            var finalArgs = result;
            while (_changedEventArgsQueue.TryDequeue(out result))
            {
                switch (result.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        finalArgs = new NotifyCollectionChangedEventArgs<T>(finalArgs.Action,
                            finalArgs.NewItems.Concat(result.NewItems).ToArray());
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        RecentModelCount = 0; //重设计数器，但是不需要清空缓存
                        _ringBufferCounter.Reset();
                        finalArgs = result;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var finalArgsNewItems = finalArgs.NewItems;
            if (finalArgsNewItems.Count == 0)
            {
                regions = null;
                return false;
            }

            regions = TryFlushModels(finalArgsNewItems);
            return true;
        }

/*  环形缓冲会增加头复制节点，也就是实际节点数量=1+总数量，增加的头节点是对最后一个节点的复制。
    这样，即使环形缓冲越过数组结尾，在第二段只有一个点，也可以和首个点形成连接
    当首次启动时，缓冲从1位置开始，_123…………；
    当越过末尾缓冲时，ringbuffer变为 9910023…………*/

        /*考虑两种种更新情况：
 1. head tail都在区间内，此时正常更新
 2. 只要head触及array末端，拷贝延长一个点，在环形开始处提前一个点。
        但是，如果恰好一次更新刚好位于结尾，两个区域将没有直接联系，所以拷贝的方式为：在前后两个额外索引位拷贝两次最后一个点位
所以，全局刷新也需要延长、提前拷贝；所有的更新必须使用映射地址。*/
        /// <summary>
        /// 将待添加的模型转换为gpu缓冲需要更新的区域
        /// </summary>
        /// <returns>update source</returns>
        private IList<GPUBufferRegion<TK>> TryFlushModels(IList<T> appendModels)
        {
            var updateRegions = new List<GPUBufferRegion<TK>>(2);
            long pendingPointsCount = appendModels.Count;
            var bufferLength = pendingPointsCount * _modelSize;
            var dirtRegions = _ringBufferCounter.AddDifference((uint)bufferLength).ToArray(); //防止重复添加
            this.RecentModelCount = (uint)(_ringBufferCounter.Length / _modelSize);
            var firstDirtRegion = dirtRegions[0];
            var firstDirtRegionLength = firstDirtRegion.Length;
            var floats = new TK[firstDirtRegionLength];
            var firstUpdateRegion = new GPUBufferRegion<TK>
            {
                Low = firstDirtRegion.Tail + _modelSize,
                High = firstDirtRegion.Head + _modelSize,
                Data = floats
            };
            //重置拷贝起始，表示脏区域已经达到或跨过环形缓冲而丢弃一些数据
            int pointIndex = 0;
            if (pendingPointsCount > _maxModelCount)
            {
                //如果大于可用长度，重置点集合的索引和长度
                pointIndex = (int)(pendingPointsCount - _maxModelCount);
                pendingPointsCount = _maxModelCount;
            }

            long index;
            int modelIndex = 0;
            T point = default;
            TK[] firstRegionData = new TK[_modelSize];
            while (modelIndex < firstDirtRegionLength / _modelSize)
            {
                point = appendModels[pointIndex];
                index = modelIndex * _modelSize;
                firstRegionData = _modelToFloatsMapping.Invoke(point);
                Array.Copy(firstRegionData, 0, floats, index, _modelSize);
                pointIndex++;
                modelIndex++;
            }

            //如果只是刚好到达容量，只会复制指定的模型填充到头，但是不会进行多条渲染（此时ringbuffer还没越过结尾）
            updateRegions.Add(firstUpdateRegion);
            //头已经顶到ringbuffer结尾，此时需要复制最后一个模型到下一行的起始；并且说明可能有两个脏区域
            if (firstDirtRegion.Head >= _ringBufferCounter.Capacity - 1)
            {
                var secondRegion = new GPUBufferRegion<TK>() { Low = 0, };
                if (dirtRegions.Length == 1)
                {
                    //只有一个更新区域，第二个更新区域只填充预缓存
                    secondRegion.High = _modelSize - 1;
                    secondRegion.Data = firstRegionData;
                }
                else
                {
                    var secondDirtRegion = dirtRegions[1];
                    secondRegion.High = secondDirtRegion.Head + _modelSize;
                    var secondRegionData = new TK[secondDirtRegion.Length + _modelSize]; //复制首节点
                    Array.Copy(firstRegionData, 0, secondRegionData, 0, _modelSize);
                    long s = _modelSize;
                    while (modelIndex < pendingPointsCount)
                    {
                        point = appendModels[pointIndex];
                        var invoke = _modelToFloatsMapping.Invoke(point);
                        Array.Copy(invoke, 0, secondRegionData, s, _modelSize);
                        pointIndex++;
                        modelIndex++;
                        s += _modelSize;
                    }

                    secondRegion.Data = secondRegionData;
                }

                updateRegions.Add(secondRegion);
            }

            return updateRegions;
        }
    }
}