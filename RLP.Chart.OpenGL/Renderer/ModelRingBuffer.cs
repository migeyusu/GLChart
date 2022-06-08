using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using RLP.Chart.Interface;
using RLP.Chart.OpenGL.Collection;

namespace RLP.Chart.OpenGL.Renderer
{
    /// <summary>
    /// 模型环形缓冲，允许存储指定的数据模型到环形缓冲。
    /// </summary>
    public class ModelRingBuffer<T, K> where K : struct
    {
        /// <summary>
        /// 模型到比特数组的映射
        /// </summary>
        private Func<T, K[]> _modelToFloatsMapping;

        /// <summary>
        /// 有效填充区域
        /// </summary>
        public IList<RingBufferCounter.Region> EffectRegions { get; set; }

        /// <summary>
        /// 最大实体数量
        /// </summary>
        public uint MaxModelCount { get; set; }

        /// <summary>
        /// 模型大小
        /// </summary>
        public uint ModelSize { get; private set; }

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

        public ModelRingBuffer()
        {
        }

        /// <summary>
        /// 分配空间
        /// </summary>
        /// <param name="maxCount">最大模型数量</param>
        /// <param name="modelSize">模型导出数组的大小，以<see cref="K"/>为单位</param>
        /// <param name="modelToFloatsMapping">模型导出数组的函数</param>
        public ModelRingBuffer(uint maxCount, uint modelSize, Func<T, K[]> modelToFloatsMapping)
        {
            this.Allocate(maxCount, modelSize, modelToFloatsMapping);
        }


        /// <summary>
        /// 分配空间
        /// </summary>
        /// <param name="maxCount">最大模型数量</param>
        /// <param name="modelSize">模型导出数组的大小，以<see cref="K"/>为单位</param>
        /// <param name="modelToFloatsMapping">模型导出数组的函数</param>
        public void Allocate(uint maxCount, uint modelSize, Func<T, K[]> modelToFloatsMapping)
        {
            _modelToFloatsMapping = modelToFloatsMapping;
            this.MaxModelCount = maxCount;
            ModelSize = modelSize;
            var deviceContainsCount = maxCount + 2; //为了使得环形缓冲的分块绘制点位不断，在gpu缓冲中必须加首尾的副本节点
            DeviceBufferSize = deviceContainsCount * modelSize;
            _ringBufferCounter = new RingBufferCounter((int)(maxCount * modelSize));
        }

        public void SendChange(NotifyCollectionChangedEventArgs<T> args)
        {
            _changedEventArgsQueue.Enqueue(args);
        }

        public IEnumerable<GPUBufferRegion<K>> Flush()
        {
            if (_changedEventArgsQueue.IsEmpty)
            {
                return Enumerable.Empty<GPUBufferRegion<K>>();
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
                        EffectRegions = default;
                        _ringBufferCounter.Reset();
                        finalArgs = result;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var finalArgsNewItems = finalArgs.NewItems;
            return FlushModels(finalArgsNewItems);
        }

        /// <summary>
        /// 将待添加的模型转换为gpu缓冲区间
        /// </summary>
        /// <returns>update source</returns>
        private IEnumerable<GPUBufferRegion<K>> FlushModels(IList<T> appendModels)
        {
            var updateRegions = new List<GPUBufferRegion<K>>(2);
            long pendingPointsCount = appendModels.Count;
            if (pendingPointsCount > 0)
            {
                var bufferLength = pendingPointsCount * ModelSize;
                var dirtRegions = _ringBufferCounter.AddDifference((uint)bufferLength).ToArray(); //防止重复添加
                this.EffectRegions = _ringBufferCounter.ContiguousRegions.ToArray();
                this.RecentModelCount = (uint)(_ringBufferCounter.Length / ModelSize);
                var firstDirtRegion = dirtRegions[0];
                var firstDirtRegionLength = firstDirtRegion.Length;
                if (firstDirtRegion.Head < _ringBufferCounter.Capacity - 1)
                {
                    /*由于更新的返回脏区域数组是按序的，首个脏区域如果没有达到环形缓冲的长度，
                         说明更新只限于小范围内，可以直接全部拷贝*/
                    var cells = new K[firstDirtRegionLength];
                    var updateRegion = new GPUBufferRegion<K>
                    {
                        Low = firstDirtRegion.Tail + ModelSize,
                        High = firstDirtRegion.Head + ModelSize,
                        Data = cells
                    };
                    long index;
                    for (int k = 0; k < pendingPointsCount; k++)
                    {
                        var point = appendModels[k];
                        index = k * ModelSize;
                        var invoke = _modelToFloatsMapping.Invoke(point);
                        Array.Copy(invoke, 0, cells, index, ModelSize);
                        /*floats[index] = point.X;
                        floats[index + 1] = point.Y;*/
                    }

                    updateRegions.Add(updateRegion);
                }
                else
                {
                    //表示脏区域已经达到或跨过环形缓冲
                    int pointIndex = 0;
                    if (pendingPointsCount > MaxModelCount)
                    {
                        //如果大于可用长度，重置点集合的索引和长度
                        pointIndex = (int)(pendingPointsCount - MaxModelCount);
                        pendingPointsCount = MaxModelCount;
                    }

                    //延长复制，因为合并渲染的需要
                    var floats = new K[firstDirtRegionLength + ModelSize];
                    var updateRegion = new GPUBufferRegion<K>
                    {
                        Low = firstDirtRegion.Tail + ModelSize,
                        High = firstDirtRegion.Head + ModelSize + ModelSize,
                        Data = floats
                    };

                    long index;
                    T point = default;
                    int loopIndex = 0;
                    K[] dataBytes = new K[ModelSize];
                    while (loopIndex < firstDirtRegionLength / ModelSize)
                    {
                        point = appendModels[pointIndex];
                        index = loopIndex * ModelSize;
                        dataBytes = _modelToFloatsMapping.Invoke(point);
                        Array.Copy(dataBytes, 0, floats, index, ModelSize);
                        /*floats[index] = point.X;
                        floats[index + 1] = point.Y;*/
                        pointIndex++;
                        loopIndex++;
                    }

                    //如果只是刚好到达容量，只会复制指定的模型填充到头尾，但是不会进行多条渲染（此时ringbuffer还没越过结尾）
                    //复制最后一个模型
                    index = loopIndex * ModelSize;
                    Array.Copy(dataBytes, 0, floats, index, ModelSize);
                    updateRegions.Add(updateRegion);
                    var secondRegion = new GPUBufferRegion<K>() { Low = 0, };
                    if (dirtRegions.Length == 1)
                    {
                        secondRegion.High = ModelSize - 1;
                        secondRegion.Data = dataBytes; //new[] { point.X, point.Y };
                    }
                    else
                    {
                        var secondDirtRegion = dirtRegions[1];
                        secondRegion.High = secondDirtRegion.Head + ModelSize;
                        var floats1 = new K[secondDirtRegion.Length + ModelSize]; //复制首节点
                        Array.Copy(dataBytes, 0, floats1, 0, ModelSize);
                        /*floats1[0] = point.X;
                        floats1[1] = point.Y;*/
                        long s = ModelSize;
                        while (loopIndex < pendingPointsCount)
                        {
                            point = appendModels[pointIndex];
                            var invoke = _modelToFloatsMapping.Invoke(point);
                            Array.Copy(invoke, 0, floats1, s, ModelSize);
                            /*floats1[s] = point.X;
                            floats1[s + 1] = point.Y;*/
                            pointIndex++;
                            loopIndex++;
                            s += ModelSize;
                        }

                        secondRegion.Data = floats1;
                    }

                    updateRegions.Add(secondRegion);
                }
            }

            return updateRegions;
        }
    }
}