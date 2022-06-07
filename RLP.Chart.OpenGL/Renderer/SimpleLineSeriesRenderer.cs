using System;

namespace RLP.Chart.OpenGL.Renderer
{
    public class SimpleLineSeriesRenderer : SeriesShaderRenderer<SimpleLineRenderer>
    {

        private float _lineThickness = 1;

        public float LineThickness
        {
            get => _lineThickness;
            set { throw new NotSupportedException(); }
        }

        public SimpleLineSeriesRenderer(Shader shader) : base(shader)
        {
        }

        public override bool PreviewRender()
        {
            return base.PreviewRender();
        }
    }
}