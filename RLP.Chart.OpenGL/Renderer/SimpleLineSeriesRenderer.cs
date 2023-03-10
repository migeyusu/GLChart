using System;
using OpenTkWPFHost.Core;

namespace RLP.Chart.OpenGL.Renderer
{
    public class SimpleLineSeriesRenderer : SeriesShaderRenderer<SimpleLineRenderer>
    {
        public SimpleLineSeriesRenderer(Shader shader) : base(shader)
        {
        }

        public override bool PreviewRender()
        {
            return base.PreviewRender();
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