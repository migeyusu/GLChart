using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xaml.Behaviors;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTkWPFHost.Abstraction;
using OpenTkWPFHost.Core;
using RLP.Chart.Interface;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Abstraction;
using RLP.Chart.OpenGL.Collection;
using Buffer = System.Buffer;

namespace RLP.Chart.OpenGL.Renderer
{
    public interface IChannel : IGeometry
    {
        IPoint3D[] Points { get; set; }
    }

    public struct Channel
    {
        public Point3D[] Points { get; set; }
    }

    /// <summary>
    /// 模型环形缓冲，允许存储指定的数据模型到环形缓冲。
    /// </summary>
    public class ModelRingBuffer<T, K> where K : struct
    {
        /// <summary>
        /// 模型到比特数组的映射
        /// </summary>
        private readonly Func<T, K[]> _modelToFloatsMapping;

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
        public uint ModelSize { get; }

        /// <summary>
        /// 当前模型数量
        /// </summary>
        public uint RecentModelCount { get; set; }

        /// <summary>
        /// gpu 缓冲区大小
        /// </summary>
        public long DeviceBufferSize { get; set; }

        private readonly RingBufferCounter _ringBufferCounter;

        private readonly ConcurrentQueue<NotifyCollectionChangedEventArgs<T>> _changedEventArgsQueue =
            new ConcurrentQueue<NotifyCollectionChangedEventArgs<T>>();

        public ModelRingBuffer(uint maxCount, uint modelSize, Func<T, K[]> modelToFloatsMapping)
        {
            _modelToFloatsMapping = modelToFloatsMapping;
            this.MaxModelCount = maxCount;
            ModelSize = modelSize;
            var deviceContainsCount = maxCount + 2;
            //为了使得环形缓冲的分块绘制点位不断，在gpu缓冲中必须加首尾的副本节点
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
            return WritePoints(finalArgsNewItems);
        }

        /// <summary>
        /// 冲洗等待的点位到设备内存，缓冲刷新的入口
        /// </summary>
        /// <returns>update source</returns>
        private IEnumerable<GPUBufferRegion<K>> WritePoints(IList<T> appendModels)
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
                    while (loopIndex < firstDirtRegionLength / ModelSize)
                    {
                        point = appendModels[pointIndex];
                        index = loopIndex * ModelSize;
                        var bytes = _modelToFloatsMapping.Invoke(point);
                        Array.Copy(bytes, 0, floats, index, ModelSize);
                        /*floats[index] = point.X;
                        floats[index + 1] = point.Y;*/
                        pointIndex++;
                        loopIndex++;
                    }

                    index = loopIndex * ModelSize;
                    var dataBytes = _modelToFloatsMapping.Invoke(point);
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
                        var floats1 = new K[secondDirtRegion.Length + ModelSize];
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

    /// <summary>
    /// single channel renderer
    /// </summary>
    public class ChannelRenderer : IShaderRendererItem, IGeometryRenderer<IChannel>
    {
        /// <summary>
        /// 通道宽度
        /// </summary>
        public uint ChannelWidth
        {
            get => _channelWidth;
            set
            {
                if (value < 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _channelWidth = value;
            }
        }

        public uint MaxChannelCount { get; set; }

        public ChannelRenderer()
        {
        }

        public void Initialize(IGraphicsContext context)
        {
            if (IsInitialized)
            {
                return;
            }

            _channelBuffer = new ModelRingBuffer<Channel, float>(this.MaxChannelCount, ChannelWidth, (channel =>
            {
                
            }));
            this.IsInitialized = true;
        }

        public bool PreviewRender()
        {
            return true;
        }

        public void Render(GlRenderEventArgs args)
        {
        }

        public void Resize(PixelSize size)
        {
        }

        public void Uninitialize()
        {
        }

        public bool IsInitialized { get; private set; }

        public Guid Id { get; } = Guid.NewGuid();

        public bool RenderEnable { get; private set; } = true;

        private ModelRingBuffer<Channel,float> _channelBuffer;
        
        public void ApplyDirective(RenderDirective directive)
        {
        }

        private Shader _shader;

        private uint _channelWidth;

        public void BindShader(Shader shader)
        {
            this._shader = shader;
        }

        public void AddGeometry(IChannel geometry)
        {
            
        }

        public void AddGeometries(IList<IChannel> geometries)
        {
            
        }

        public void ResetWith(IList<IChannel> geometries)
        {
        }

        public void ResetWith(IChannel geometry)
        {
        }

        public void Clear()
        {
        }
    }

    public class ChannelSeriesRenderer : SeriesShaderRenderer<ChannelRenderer>
    {
        public ChannelSeriesRenderer(Shader shader) : base(shader)
        {
        }
    }

    /// <summary>
    /// 基于3d 坐标轴的渲染器
    /// </summary>
    public class Coordinate3DRenderer : IRenderer
    {
        public Color4 BackgroundColor { get; set; } = Color4.White;

        /// <summary>
        /// 自适应Y轴的默认区间，当界面内没有元素时显示该区间
        /// </summary>
        public ScrollRange AxisYRange { get; set; } = new ScrollRange(0, 100);

        public Region3D TargetRegion { get; set; }

        public IReadOnlyCollection<BaseRenderer> Series =>
            new ReadOnlyCollection<BaseRenderer>(_renderSeriesCollection);

        private readonly IList<BaseRenderer> _renderSeriesCollection;

        /// <summary>
        /// 渲染快照
        /// </summary>
        private IList<BaseRenderer> _rendererSeriesSnapList = new List<BaseRenderer>();

        private IGraphicsContext _context;

        public void Initialize(IGraphicsContext context)
        {
            if (IsInitialized)
            {
                return;
            }

            this._context = context;
            GL.Enable(EnableCap.DepthTest);
            foreach (var renderer in _renderSeriesCollection)
            {
                renderer.Initialize(context);
            }

            IsInitialized = true;
        }

        public bool PreviewRender()
        {
            return true;
        }

        private Matrix4 _transform = Matrix4.Identity;

        public void Render(GlRenderEventArgs args)
        {
            GL.ClearColor(BackgroundColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            if (_rendererSeriesSnapList.All(series => !series.AnyReadyRenders()))
            {
                return;
            }

            foreach (var seriesItem in _rendererSeriesSnapList)
            {
                seriesItem.ApplyDirective(new RenderDirective(_transform));
                seriesItem.Render(args);
            }
        }

        public void Resize(PixelSize size)
        {
            GL.Viewport(0, 0, size.Width, size.Height);
        }

        public void Uninitialize()
        {
            foreach (var baseRenderer in _renderSeriesCollection)
            {
                baseRenderer.Uninitialize();
            }
        }

        public bool IsInitialized { get; private set; }
    }
}