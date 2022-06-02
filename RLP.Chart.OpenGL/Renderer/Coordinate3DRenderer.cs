using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// gpu 环形缓冲
    /// </summary>
    public class DeviceRingBuffer<T> where T : struct, IEnumerable<float>
    {
        private int _deviceBufferSize;

        private RingBufferCounter _ringBufferCounter;

        private readonly ConcurrentQueue<NotifyCollectionChangedEventArgs<Point2D>> _changedEventArgsQueue =
            new ConcurrentQueue<NotifyCollectionChangedEventArgs<Point2D>>();

        public DeviceRingBuffer(int maxCount)
        {
            var i = Marshal.SizeOf<T>();
            var deviceContainsCount = maxCount + 2;
            //为了使得环形缓冲的分块绘制点位不断，在gpu缓冲中必须加首尾的副本节点
            _deviceBufferSize = deviceContainsCount * i;
            _ringBufferCounter = new RingBufferCounter(maxCount * i);
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

        public int MaxChannelCount { get; set; }

        public ChannelRenderer()
        {
        }

        public void Initialize(IGraphicsContext context)
        {
            if (IsInitialized)
            {
                return;
            }

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