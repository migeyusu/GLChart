using System;
using GLChart.Core.Renderer;
using OpenTkWPFHost.Abstraction;

namespace GLChart.Core.Abstraction
{
    /// <summary>
    /// （基于坐标系的）渲染项
    /// </summary>
    public interface IRendererItem : IRenderer
    {
        Guid Id { get; }

        bool RenderEnable { get; }

        /// <summary>
        /// 在渲染前调用，表示应用指令
        /// </summary>
        /// <param name="directive"></param>
        void ApplyDirective(RenderDirective directive);
    }
}