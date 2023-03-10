using System;
using OpenTK;
using OpenTkWPFHost.Core;

namespace RLP.Chart.OpenGL.Renderer
{
    /// <summary>
    /// 高级线渲染组
    /// </summary>
    public class AdvancedLineSeriesRenderer : SeriesShaderRenderer<AdvancedLineRenderer>
    {
        public AdvancedLineSeriesRenderer(Shader shader) : base(shader)
        {
        }

        protected override void ConfigShader(GlRenderEventArgs args)
        {
            base.ConfigShader(args);
            //必要的先决信息
            Shader.SetVec2("u_resolution", new Vector2(args.Width, args.Height));
        }

        public override void ApplyDirective(RenderDirective directive)
        {
            base.ApplyDirective(directive);
            if (directive is RenderDirective2D directive2D)
            {
                this.Shader.SetMatrix4("transform", directive2D.Transform);
            }
        }
    }
}