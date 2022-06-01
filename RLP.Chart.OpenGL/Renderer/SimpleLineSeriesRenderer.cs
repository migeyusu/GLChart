using System;

namespace RLP.Chart.OpenGL.Renderer
{
    public class SimpleLineSeriesRenderer : SeriesShaderRenderer<SimpleLineRenderer>
    {
        /// <summary>
        /// 最大线宽
        /// </summary>
        public float MinLineThickness { get; private set; } = 1; //无法修改

        /// <summary>
        /// 最小线宽
        /// </summary>
        public float MaxLineThickness { get; private set; } = 1; //无法修改

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