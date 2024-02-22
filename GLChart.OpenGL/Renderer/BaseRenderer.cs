using System;
using GLChart.Core.Abstraction;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Core;

namespace GLChart.Core.Renderer
{
    /// <summary>
    /// 基础渲染器
    /// </summary>
    public abstract class BaseRenderer : IRendererItem
    {
        public bool IsInitialized { get; protected set; }

        public Guid Id { get; protected set; }
        
        public bool RenderEnable { get; set; } = true;

        /// <summary>
        /// 是否有准备的渲染器
        /// </summary>
        /// <returns></returns>
        public abstract bool AnyReadyRenders();

        public abstract void Initialize(IGraphicsContext context);

        /// <summary>
        /// 渲染前准备，可以执行检查缓冲等
        /// </summary>
        /// <returns>true：成功刷新了缓冲区</returns>
        public abstract bool PreviewRender();

        public abstract void Render(GlRenderEventArgs args);

        public void Resize(PixelSize size)
        {
            //no implement
        }

        /// <summary>
        /// 应用绘制指令
        /// </summary>
        /// <param name="directive"></param>
        public abstract void ApplyDirective(RenderDirective directive);

        public abstract void Uninitialize();
    }
}