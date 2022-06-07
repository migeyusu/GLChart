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
using Buffer = System.Buffer;

namespace RLP.Chart.OpenGL.Renderer
{
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

        /// <summary>
        /// 只显示该范围内的
        /// </summary>
        public Region3D TargetRegion { get; set; }

        public Matrix4 View { get; set; }

        /// <summary>
        /// 模型位置
        /// </summary>
        public Matrix4 ModelPosition { get; set; }

        /// <summary>
        /// 投影
        /// </summary>
        public Matrix4 Projection { get; set; }

        public IReadOnlyCollection<BaseRenderer> Series =>
            new ReadOnlyCollection<BaseRenderer>(_renderSeriesCollection);

        private readonly IList<BaseRenderer> _renderSeriesCollection;

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
            //检查未初始化，当预渲染ok且渲染可用时表示渲染可用
            foreach (var renderSeries in _renderSeriesCollection)
            {
                if (!renderSeries.IsInitialized)
                {
                    renderSeries.Initialize(this._context);
                }

                renderSeries.PreviewRender();
            }

            return true;
        }

        public Coordinate3DRenderer(IList<BaseRenderer> renderSeriesCollection)
        {
            _renderSeriesCollection = renderSeriesCollection;
        }


        private Matrix4 _transform = Matrix4.Identity;

        public void Render(GlRenderEventArgs args)
        {
            GL.ClearColor(BackgroundColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            var identity = Matrix4.Identity;
            identity *= Projection;
            identity *= View;
            identity *= ModelPosition;
            foreach (var seriesItem in _renderSeriesCollection.Where(renderer => renderer.AnyReadyRenders()))
            {
                seriesItem.ApplyDirective(new RenderDirective(identity));
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