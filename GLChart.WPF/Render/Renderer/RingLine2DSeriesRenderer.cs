using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Core;

namespace GLChart.WPF.Render.Renderer
{
    /// <summary>
    /// 线渲染集合
    /// </summary>
    public class RingLine2DSeriesRenderer : SeriesShaderRenderer<RingLine2DRenderer>
    {
        public RingLine2DSeriesRenderer(Shader shader) : base(shader)
        {
        }

        private int _vertexArrayObject;

        public override void Initialize(IGraphicsContext context)
        {
            base.Initialize(context);
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
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

        public override void Render(GlRenderEventArgs args)
        {
            if (RenderWorkingList.Count == 0)
            {
                return;
            }

            ConfigShader(args);
            GL.BindVertexArray(_vertexArrayObject);
            foreach (var rendererItem in RenderWorkingList)
            {
                rendererItem.Render(args);
            }
        }

        public override void Uninitialize()
        {
            base.Uninitialize();
            if (_vertexArrayObject != 0)
            {
                GL.DeleteVertexArray(_vertexArrayObject);
            }
        }
    }
}