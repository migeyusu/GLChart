using System;
using OpenTK;
using OpenTkWPFHost.Core;

namespace RLP.Chart.OpenGL.Renderer
{
    /// <summary>
    /// 
    /// </summary>
    public class AdvancedLineSeriesRenderer : SeriesShaderRenderer<AdvancedLineRenderer>
    {
        /// <summary>
        /// 最大线宽
        /// </summary>
        public float MinLineThickness { get; private set; } = 1; //无法修改

        /// <summary>
        /// 最小线宽
        /// </summary>
        public float MaxLineThickness { get; private set; } = 10; //无法修改

        private float _lineThickness = 1;

        public float LineThickness
        {
            get => _lineThickness;
            set { throw new NotSupportedException(); }
        }

        public AdvancedLineSeriesRenderer(Shader shader) : base(shader)
        {
        }

        public override void Render(GlRenderEventArgs args)
        {
            base.Render(args);
        }

        protected override void ApplyShader(GlRenderEventArgs args)
        {
            base.ApplyShader(args);
            Shader.SetFloat("u_thickness", LineThickness);
            Shader.SetVec2("u_resolution", new Vector2(args.Width, args.Height));
        }
    }
}