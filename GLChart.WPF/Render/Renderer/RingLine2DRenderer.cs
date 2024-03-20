using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using GLChart.WPF.Base;
using GLChart.WPF.Render.Allocation;
using GLChart.WPF.Render.CollisionDetection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Core;

namespace GLChart.WPF.Render.Renderer
{
    /// <summary>
    /// 环形线条渲染，基于三角形绘制。
    /// <para>使用环形缓冲以优化显存分配，不需要频繁更新显存中的数据，适合时序数据</para>
    /// <para>不支持<see cref="IGeometryCollection{T}.Insert"/>和<see cref="IGeometryCollection{T}.Remove"/>类型的操作</para>
    /// </summary>
    public class RingLine2DRenderer : Line2DRenderer
    {
        private int _pointsCountLimit;

        public int PointCountLimit
        {
            get => _pointsCountLimit;
            set
            {
                _pointsCountLimit = value;
                if (this.IsInitialized)
                {
                    Trace.TraceWarning("Graphic buffer will reinitialize");
                }

                IsInitialized = false;
            }
        }

        private int _shaderStorageBufferObject;

        private int _vertexArrayObject;

        private readonly ModelRingBuffer<IPoint2D, float> _pointsBuffer = new ModelRingBuffer<IPoint2D, float>();

        /// <summary>
        /// 
        /// </summary>
        public RingLine2DRenderer(ICollisionPoint2D collisionPoint2D, int pointsCountLimit = 1024) : base(
            collisionPoint2D)
        {
            _pointsCountLimit = pointsCountLimit;
        }

        public override void Initialize(IGraphicsContext context)
        {
            if (IsInitialized)
            {
                return;
            }

            _pointsBuffer.Initialize((uint)PointCountLimit, 2, (point2D =>
            {
                var foo = new float[2];
                foo[0] = point2D.X;
                foo[1] = point2D.Y;
                return foo;
            }));
            _shaderStorageBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _shaderStorageBufferObject);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, (int)_pointsBuffer.DeviceBufferSize * SizeFloat,
                IntPtr.Zero,
                BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, _shaderStorageBufferObject);
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            this.IsInitialized = true;
        }

        public override bool PreviewRender()
        {
            if (!_pointsBuffer.TryFlush(out var updateRegions))
            {
                return false;
            }

            if (updateRegions.Count > 0)
            {
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _shaderStorageBufferObject);
                foreach (var updateRegion in updateRegions)
                {
                    GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)(updateRegion.Low * SizeFloat),
                        (IntPtr)(updateRegion.Length * SizeFloat), updateRegion.Data);
                }
            }

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _shaderStorageBufferObject);
            var floats = new float[_pointsBuffer.DeviceBufferSize];
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0,
                (int)_pointsBuffer.DeviceBufferSize * SizeFloat, floats);
            return true;
        }

        public override void Render(GlRenderEventArgs args)
        {
            if (_pointsBuffer.RecentModelCount < 2)
            {
                return;
            }

            Debug.Assert(Shader != null, nameof(Shader) + " != null");
            Shader.SetFloat("u_thickness", Thickness);
            Shader.SetColor("linecolor", Color4);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _shaderStorageBufferObject);
            GL.BindVertexArray(_vertexArrayObject);
            var drawRegions = _pointsBuffer.EffectRegions;
            var firstDrawRegion = drawRegions[0];
            var count1 = firstDrawRegion.Length / 2;
            if (drawRegions.Count == 1)
            {
                Shader.SetInt("startIndex", firstDrawRegion.Tail);
                Shader.SetInt("maxLineIndex", (firstDrawRegion.Head + 1) / 2);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * count1);
            }
            else
            {
                Shader.SetInt("maxLineIndex", (firstDrawRegion.Head + 1) / 2);
                Shader.SetInt("startIndex", firstDrawRegion.Tail / 2);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * count1);
                var secondDrawRegion = drawRegions[1];
                var count2 = secondDrawRegion.Length / 2 + 1;
                Shader.SetInt("maxLineIndex", (secondDrawRegion.Head + 1) / 2);
                Shader.SetInt("startIndex", -1);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * count2);
            }
        }

        /// <summary>
        /// 冲洗之前操作到设备缓冲，在opengl上下文调用
        /// </summary>
        /// <returns>true:更新了设备缓冲；false：未更新</returns>
        public override void Add(IPoint2D point)
        {
            _pointsBuffer.SendChange(
                NotifyCollectionChangedEventArgs<IPoint2D>.AppendArgs(new Point2D(point.X, point.Y)));
            CollisionPoint2D.Add(point);
        }

        public override void AddRange(IList<IPoint2D> points)
        {
            _pointsBuffer.SendChange(
                NotifyCollectionChangedEventArgs<IPoint2D>.AppendRangeArgs(points
                    .Select((point => new Point2D(point.X, point.Y))).Cast<IPoint2D>().ToArray()));
            CollisionPoint2D.AddRange(points);
        }

        public override void Remove(IPoint2D geometry)
        {
            throw new NotSupportedException();
        }

        public override void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public override void Insert(int index, IPoint2D geometry)
        {
            throw new NotSupportedException();
        }

        public override void ResetWith(IList<IPoint2D> geometries)
        {
            _pointsBuffer.SendChange(new NotifyCollectionChangedEventArgs<IPoint2D>(
                NotifyCollectionChangedAction.Reset,
                geometries.Select(point => new Point2D(point.X, point.Y))
                    .Cast<IPoint2D>()
                    .ToArray()));
            CollisionPoint2D.ResetWith(geometries);
        }

        public override void ResetWith(IPoint2D geometry)
        {
            _pointsBuffer.SendChange(
                new NotifyCollectionChangedEventArgs<IPoint2D>(NotifyCollectionChangedAction.Reset,
                    new Point2D(geometry.X, geometry.Y)));
            CollisionPoint2D.ResetWith(geometry);
        }

        public override void Clear()
        {
            _pointsBuffer.SendChange(NotifyCollectionChangedEventArgs<IPoint2D>.ResetArgs);
            CollisionPoint2D.Clear();
        }

        public override void Uninitialize()
        {
            if (_shaderStorageBufferObject != 0)
            {
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
                GL.DeleteBuffer(_shaderStorageBufferObject);
            }

            if (_vertexArrayObject != 0)
            {
                GL.DeleteVertexArray(_vertexArrayObject);
            }

            IsInitialized = false;
        }
    }
}