using System;
using OpenTkWPFHost.Abstraction;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.Abstraction
{
    /// <summary>
    /// （基于坐标系的）项渲染器
    /// </summary>
    public interface IRendererItem : IRenderer
    {
        Guid Id { get; }
        bool RenderEnable { get; }
        void ApplyDirective(RenderDirective directive);
    }
}