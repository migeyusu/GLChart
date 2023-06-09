using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Core;
using RLP.Chart.Interface;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Abstraction;
using RLP.Chart.OpenGL.Collection;
using RLP.Chart.OpenGL.CollisionDetection;

namespace RLP.Chart.OpenGL.Renderer
{
    /// <summary>
    /// 线条渲染，基于三角形绘制
    /// </summary>
    public class LineRenderer : IShaderRendererItem, ILine
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string Title { get; set; }

        public bool RenderEnable
        {
            get => _renderEnable;
            set => _renderEnable = value;
        }

        public bool IsInitialized
        {
            get => _isInitialized;
            private set => _isInitialized = value;
        }

        public int PointCountLimit
        {
            get => _pointsCountLimit;
            set
            {
                _pointsCountLimit = value;
                if (_isInitialized)
                {
                    throw new Exception($"Don't support set {nameof(PointCountLimit)} after initialized");
                }
            }
        }

        private Color4 _color4 = Color4.Blue;

        public System.Drawing.Color LineColor
        {
            get => System.Drawing.Color.FromArgb(_color4.ToArgb());
            set => _color4 = new Color4(value.R, value.G, value.B, value.A);
        }

        /// <summary>
        /// 线宽
        /// </summary>
        public float Thickness { get; set; } = 2;

        /// <summary>
        /// GPU缓冲区大小
        /// </summary>
        public int DeviceBufferSize
        {
            get => _deviceBufferSize;
            private set => _deviceBufferSize = value;
        }

        public RingBufferCounter Counter { get; private set; }

        public int PointCount
        {
            get => _pointCount;
            private set => _pointCount = value;
        }

        private const int SizeFloat = sizeof(float);

        protected int ShaderStorageBufferObject;

        protected int VertexArrayObject;

        private readonly ConcurrentQueue<NotifyCollectionChangedEventArgs<Point2D>> _changedEventArgsQueue =
            new ConcurrentQueue<NotifyCollectionChangedEventArgs<Point2D>>();

        /// <summary>
        /// 等待加入的点位队列，点位的添加只能在opengl上下文实现
        /// </summary>
        // private readonly List<Point> _pendingList = new List<Point>(10);
        private int _pointsCountLimit = 4096;

        private volatile bool _isInitialized;

        private volatile bool _renderEnable;

        private volatile int _deviceBufferSize;

        private volatile int _pointCount;

        internal ICollisionPoint2D CollisionLayer { get; }

        private IList<RingBufferCounter.Region> DrawRegions { get; set; }

        // private readonly ModelRingBuffer<IPoint2D, float> _pointsBuffer = new ModelRingBuffer<IPoint2D, float>();

        /// <summary>
        /// 
        /// </summary>
        public LineRenderer(ICollisionPoint2D collisionLayer)
        {
            CollisionLayer = collisionLayer;
            this.RenderEnable = true;
        }

        private Shader _shader;

        public virtual void Initialize(IGraphicsContext context)
        {
            if (IsInitialized)
            {
                return;
            }

            CalculateBufferSize(PointCountLimit);
            if (DeviceBufferSize < 1)
            {
                throw new NotSupportedException($"{PointCountLimit} cannot less than 1!");
            }

            ShaderStorageBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferObject);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, DeviceBufferSize * SizeFloat, IntPtr.Zero,
                BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, ShaderStorageBufferObject);
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            this.IsInitialized = true;
        }

        private void CalculateBufferSize(int maxPointCount)
        {
            maxPointCount++;
            var virtualBufferSize = maxPointCount * 2;
            //为了使得环形缓冲的分块绘制点位不断，在gpu缓冲中增加首尾的副本节点
            this.DeviceBufferSize = (maxPointCount + 2) * 2;
            this.Counter = new RingBufferCounter(virtualBufferSize);
            Add(new Point2D(0, 0));
            // WritePoints(new Point[] { new Point(0, 0) });
        }

        public virtual void Render(GlRenderEventArgs args)
        {
            if (PointCount < 2)
            {
                return;
            }

#if Read
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            var floats = new float[GPUBufferSize];
            GL.GetBufferSubData(BufferTarget.ArrayBuffer, (IntPtr) 0, GPUBufferSize * SizeFloat, floats);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
#endif
            _shader.SetFloat("u_thickness", Thickness);
            _shader.SetColor("linecolor", _color4);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferObject);
            GL.BindVertexArray(VertexArrayObject);
            var drawRegion = DrawRegions[0];
            if (DrawRegions.Count == 1)
            {
                _shader.SetInt("startIndex", drawRegion.Tail + 2);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * (drawRegion.Length / 2 - 1));
            }
            else
            {
                _shader.SetInt("startIndex", drawRegion.Tail + 2);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * (drawRegion.Length / 2));
                var secondDrawRegion = DrawRegions[1];
                _shader.SetInt("startIndex", secondDrawRegion.Tail);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * (secondDrawRegion.Length / 2));
            }
        }

        public bool PreviewRender()
        {
            if (_changedEventArgsQueue.IsEmpty)
            {
                return false;
            }

            while (_changedEventArgsQueue.TryDequeue(out var result))
            {
                switch (result.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        WritePoints(result.NewItems);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        PointCount = 0; //重设计数器，但是不需要清空缓存
                        DrawRegions = default;
                        Counter.Reset();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }

        public void ApplyDirective(RenderDirective directive)
        {
            //no implement
        }

        public void BindShader(Shader shader)
        {
            this._shader = shader;
        }

        public void Resize(PixelSize size)
        {
            //no implement
        }

        /// <summary>
        /// 冲洗等待的点位到设备内存，缓冲刷新的入口
        /// </summary>
        /// <returns>update source</returns>
        private void WritePoints(IList<Point2D> appendPoints)
        {
            var updateRegions = new List<GPUBufferRegion<float>>(2);
            var pendingPointsCount = appendPoints.Count;
            if (pendingPointsCount > 0)
            {
                var bufferLength = pendingPointsCount * 2;
                var dirtRegions = Counter.AddDifference((uint)bufferLength).ToArray(); //防止重复添加
                DrawRegions = Counter.ContiguousRegions.ToArray();
                this.PointCount = Counter.Length / 2;
                var firstDirtRegion = dirtRegions[0];
                var firstDirtRegionLength = firstDirtRegion.Length;
                if (firstDirtRegion.Head < Counter.Capacity - 1)
                {
                    /*由于更新的返回脏区域数组是按序的，首个脏区域如果没有达到环形缓冲的长度，
                         说明更新只限于小范围内，可以直接全部拷贝*/
                    var floats = new float[firstDirtRegionLength];
                    var updateRegion = new GPUBufferRegion<float>()
                    {
                        Low = firstDirtRegion.Tail + 2,
                        High = firstDirtRegion.Head + 2,
                        Data = floats
                    };
                    int index;
                    for (int k = 0; k < pendingPointsCount; k++)
                    {
                        var point = appendPoints[k];
                        index = k * 2;
                        floats[index] = point.X;
                        floats[index + 1] = point.Y;
                    }

                    updateRegions.Add(updateRegion);
                }
                else
                {
                    //表示脏区域已经达到或跨过环形缓冲
                    var pointIndex = 0;
                    if (pendingPointsCount > PointCountLimit)
                    {
                        //如果大于可用长度，重置点集合的索引和长度
                        pointIndex = pendingPointsCount - PointCountLimit;
                        pendingPointsCount = PointCountLimit;
                    }

                    //延长复制，因为合并渲染的需要
                    var floats = new float[firstDirtRegionLength + 2];
                    var updateRegion = new GPUBufferRegion<float>()
                    {
                        Low = firstDirtRegion.Tail + 2,
                        High = firstDirtRegion.Head + 4,
                        Data = floats
                    };

                    int index;
                    Point2D point = default;
                    int loopIndex = 0;
                    while (loopIndex < firstDirtRegionLength / 2)
                    {
                        point = appendPoints[pointIndex];
                        index = loopIndex * 2;
                        floats[index] = point.X;
                        floats[index + 1] = point.Y;
                        pointIndex++;
                        loopIndex++;
                    }

                    index = loopIndex * 2;
                    floats[index] = point.X;
                    floats[index + 1] = point.Y;
                    updateRegions.Add(updateRegion);
                    var secondRegion = new GPUBufferRegion<float>() { Low = 0, };
                    if (dirtRegions.Length == 1)
                    {
                        secondRegion.High = 1;
                        secondRegion.Data = new[] { point.X, point.Y };
                    }
                    else
                    {
                        var secondDirtRegion = dirtRegions[1];
                        secondRegion.High = secondDirtRegion.Head + 2;
                        var floats1 = new float[secondDirtRegion.Length + 2];
                        floats1[0] = point.X;
                        floats1[1] = point.Y;
                        int s = 2;
                        while (loopIndex < pendingPointsCount)
                        {
                            point = appendPoints[pointIndex];
                            floats1[s] = point.X;
                            floats1[s + 1] = point.Y;
                            pointIndex++;
                            loopIndex++;
                            s += 2;
                        }

                        secondRegion.Data = floats1;
                    }

                    updateRegions.Add(secondRegion);
                }
            }

            if (updateRegions.Count > 0)
            {
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferObject);
                foreach (var updateRegion in updateRegions)
                {
                    GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)(updateRegion.Low * SizeFloat),
                        (IntPtr)(updateRegion.Length * SizeFloat), updateRegion.Data);
                }
#if Read
                var floats = new float[GPUBufferSize];
                GL.GetBufferSubData(BufferTarget.ArrayBuffer, (IntPtr) 0, GPUBufferSize * SizeFloat, floats);
#endif
            }
        }

        /*考虑两种种更新情况：
 1. head tail都在区间内，此时正常更新
 2. 只要head触及array末端，拷贝延长一个点，在环形开始处提前一个点。
        但是，如果恰好一次更新刚好位于结尾，两个区域将没有直接联系，所以拷贝的方式为：在前后两个额外索引位拷贝两次最后一个点位
所以，全局刷新也需要延长、提前拷贝；所有的更新必须使用映射地址。*/

        /// <summary>
        /// 冲洗之前操作到设备缓冲，在opengl上下文调用
        /// </summary>
        /// <returns>true:更新了设备缓冲；false：未更新</returns>
        public void Add(IPoint2D point)
        {
            _changedEventArgsQueue.Enqueue(
                NotifyCollectionChangedEventArgs<Point2D>.AppendArgs(new Point2D(point.X, point.Y)));
            CollisionLayer.Add(point);
        }

        public void AddRange(IList<IPoint2D> points)
        {
            _changedEventArgsQueue.Enqueue(
                NotifyCollectionChangedEventArgs<Point2D>.AppendRangeArgs(points
                    .Select((point => new Point2D(point.X, point.Y))).ToArray()));
            CollisionLayer.AddRange(points);
        }

        public void ResetWith(IList<IPoint2D> geometries)
        {
            _changedEventArgsQueue.Enqueue(new NotifyCollectionChangedEventArgs<Point2D>(
                NotifyCollectionChangedAction.Reset,
                geometries.Select(point => new Point2D(point.X, point.Y)).ToArray()));
            CollisionLayer.ResetWith(geometries);
        }

        public void ResetWith(IPoint2D geometry)
        {
            _changedEventArgsQueue.Enqueue(
                new NotifyCollectionChangedEventArgs<Point2D>(NotifyCollectionChangedAction.Reset,
                    new Point2D(geometry.X, geometry.Y)));
            CollisionLayer.ResetWith(geometry);
        }

        public void Clear()
        {
            _changedEventArgsQueue.Enqueue(NotifyCollectionChangedEventArgs<Point2D>.ResetArgs);
            this.CollisionLayer.Clear();
        }

        public void Uninitialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            GL.DeleteBuffer(ShaderStorageBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);
            IsInitialized = false;
        }

        protected bool Equals(LineRenderer other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LineRenderer)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}