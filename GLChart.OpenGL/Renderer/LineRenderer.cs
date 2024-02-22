using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using GLChart.Core.Abstraction;
using GLChart.Core.CollisionDetection;
using GLChart.Interface;
using GLChart.Interface.Abstraction;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Core;

namespace GLChart.Core.Renderer
{
    /// <summary>
    /// 线条渲染，基于三角形绘制
    /// </summary>
    public class LineRenderer : IShaderRendererItem, ILine
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string Title { get; set; }

        private volatile bool _renderEnable = true;

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

        private int _pointsCountLimit;

        public int PointCountLimit
        {
            get => _pointsCountLimit;
            set
            {
                _pointsCountLimit = value;
                if (_isInitialized)
                {
                    Trace.TraceWarning("Graphic buffer will reinitialize");
                }

                _isInitialized = false;
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

        private const int SizeFloat = sizeof(float);

        protected int ShaderStorageBufferObject;

        protected int VertexArrayObject;

        private volatile bool _isInitialized;

        internal ICollisionPoint2D CollisionLayer { get; }

        private ModelRingBuffer<IPoint2D, float> _pointsBuffer = new ModelRingBuffer<IPoint2D, float>();

        /// <summary>
        /// 
        /// </summary>
        internal LineRenderer(ICollisionPoint2D collisionLayer, int pointsCountLimit = 1024)
        {
            _pointsCountLimit = pointsCountLimit;
            CollisionLayer = collisionLayer;
        }

        private Shader _shader;

        public virtual void Initialize(IGraphicsContext context)
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
            ShaderStorageBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferObject);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, (int)_pointsBuffer.DeviceBufferSize * SizeFloat,
                IntPtr.Zero,
                BufferUsageHint.StaticDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, ShaderStorageBufferObject);
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            this.IsInitialized = true;
        }

        public bool PreviewRender()
        {
            if (_pointsBuffer.TryFlush(out var updateRegions))
            {
                if (updateRegions.Count > 0)
                {
                    GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferObject);
                    foreach (var updateRegion in updateRegions)
                    {
                        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)(updateRegion.Low * SizeFloat),
                            (IntPtr)(updateRegion.Length * SizeFloat), updateRegion.Data);
                    }
                }
            }

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferObject);
            var floats = new float[_pointsBuffer.DeviceBufferSize];
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)0,
                (int)_pointsBuffer.DeviceBufferSize * SizeFloat, floats);
            Debug.WriteLine("render request");
            return true;
        }

        public virtual void Render(GlRenderEventArgs args)
        {
            if (_pointsBuffer.RecentModelCount < 2)
            {
                return;
            }

            Debug.WriteLine("rendering");
            _shader.SetFloat("u_thickness", Thickness);
            _shader.SetColor("linecolor", _color4);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferObject);
            GL.BindVertexArray(VertexArrayObject);
            var drawRegions = _pointsBuffer.EffectRegions;
            var firstDrawRegion = drawRegions[0];
            var count1 = firstDrawRegion.Length / 2;
            if (drawRegions.Count == 1)
            {
                _shader.SetInt("startIndex", firstDrawRegion.Tail);
                _shader.SetInt("maxLineIndex", (firstDrawRegion.Head + 1) / 2);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * count1);
            }
            else
            {
                _shader.SetInt("maxLineIndex", (firstDrawRegion.Head + 1) / 2);
                _shader.SetInt("startIndex", firstDrawRegion.Tail / 2);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * count1);
                var secondDrawRegion = drawRegions[1];
                var count2 = secondDrawRegion.Length / 2 + 1;
                _shader.SetInt("maxLineIndex", (secondDrawRegion.Head + 1) / 2);
                _shader.SetInt("startIndex", -1);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * count2);
            }
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
        /// 冲洗之前操作到设备缓冲，在opengl上下文调用
        /// </summary>
        /// <returns>true:更新了设备缓冲；false：未更新</returns>
        public void Add(IPoint2D point)
        {
            _pointsBuffer.SendChange(
                NotifyCollectionChangedEventArgs<IPoint2D>.AppendArgs(new Point2D(point.X, point.Y)));
            CollisionLayer.Add(point);
        }

        public void AddRange(IList<IPoint2D> points)
        {
            _pointsBuffer.SendChange(
                NotifyCollectionChangedEventArgs<IPoint2D>.AppendRangeArgs(points
                    .Select((point => new Point2D(point.X, point.Y))).Cast<IPoint2D>().ToArray()));
            CollisionLayer.AddRange(points);
        }

        public void ResetWith(IList<IPoint2D> geometries)
        {
            _pointsBuffer.SendChange(new NotifyCollectionChangedEventArgs<IPoint2D>(
                NotifyCollectionChangedAction.Reset,
                geometries.Select(point => new Point2D(point.X, point.Y))
                    .Cast<IPoint2D>()
                    .ToArray()));
            CollisionLayer.ResetWith(geometries);
        }

        public void ResetWith(IPoint2D geometry)
        {
            _pointsBuffer.SendChange(
                new NotifyCollectionChangedEventArgs<IPoint2D>(NotifyCollectionChangedAction.Reset,
                    new Point2D(geometry.X, geometry.Y)));
            CollisionLayer.ResetWith(geometry);
        }

        public void Clear()
        {
            _pointsBuffer.SendChange(NotifyCollectionChangedEventArgs<IPoint2D>.ResetArgs);
            this.CollisionLayer.Clear();
        }

        public void Uninitialize()
        {
            if (ShaderStorageBufferObject != 0)
            {
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
                GL.DeleteBuffer(ShaderStorageBufferObject);
            }

            if (VertexArrayObject != 0)
            {
                GL.DeleteVertexArray(VertexArrayObject);
            }

            IsInitialized = false;
        }

        private bool Equals(LineRenderer other)
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