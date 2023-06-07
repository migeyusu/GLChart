using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Abstraction;
using OpenTkWPFHost.Core;

namespace RLP.Chart.OpenGL.Renderer
{
    /*2021.12.22：
     已知问题：不能设置零一下的Y轴起点*/

    public class Coordinate2DRenderer : IRenderer
    {
        public Color4 BackgroundColor { get; set; } = Color4.White;

        public IReadOnlyCollection<BaseRenderer> Series =>
            new ReadOnlyCollection<BaseRenderer>(RenderSeriesCollection);

        protected readonly IList<BaseRenderer> RenderSeriesCollection;

        public Coordinate2DRenderer(IList<BaseRenderer> renderSeriesCollection)
        {
            RenderSeriesCollection = renderSeriesCollection;
        }


        protected IGraphicsContext Context;

        public virtual void Initialize(IGraphicsContext context)
        {
            if (IsInitialized)
            {
                return;
            }

            this.Context = context;
            GL.Enable(EnableCap.DepthTest);
            foreach (var renderer in RenderSeriesCollection)
            {
                renderer.Initialize(context);
            }

            IsInitialized = true;
        }

        public virtual bool PreviewRender()
        {
            return true;
        }
        
        public virtual void Render(GlRenderEventArgs args)
        {
            GL.ClearColor(BackgroundColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (var seriesItem in RenderSeriesCollection.Where((renderer => renderer.AnyReadyRenders())))
            {
                seriesItem.ApplyDirective(new RenderDirective());
                seriesItem.Render(args);
            }
        }

        public virtual void Resize(PixelSize size)
        {
            GL.Viewport(0, 0, size.Width, size.Height);
        }

        public virtual void Uninitialize()
        {
            foreach (var coordinateRendererSeries in RenderSeriesCollection)
            {
                coordinateRendererSeries.Uninitialize();
            }
        }

        public bool IsInitialized { get; protected set; }
    }
}