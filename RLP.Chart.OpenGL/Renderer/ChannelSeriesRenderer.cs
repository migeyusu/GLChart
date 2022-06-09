using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTkWPFHost.Core;

namespace RLP.Chart.OpenGL.Renderer
{
    /// <summary>
    /// 表示颜色渐变范围
    /// </summary>
    public class ColorRange
    {
        public Color4 Start { get; set; }

        public Color4 End { get; set; }

        public Vector3 Gradual
        {
            get { return new Vector3(End.R - Start.R, End.G - Start.G, End.B - Start.B); }
        }
    }

    public class ChannelSeriesRenderer : SeriesShaderRenderer<ChannelRenderer>
    {
        public ColorRange ColorRange { get; set; } = new ColorRange() { Start = Color4.Black, End = Color4.Red };


        /// <summary>
        /// 模型位置
        /// </summary>
        public Matrix4 ModelPosition { get; set; } = Matrix4.Identity;

        /// <summary>
        /// z轴最大值
        /// </summary>
        public float ZHighest { get; set; } = 230;

        public ChannelSeriesRenderer(Shader shader) : base(shader)
        {
        }

        protected override void ConfigShader(GlRenderEventArgs args)
        {
            base.ConfigShader(args);
            Shader.SetFloat("highest", ZHighest);
            Shader.SetVector3("colorgradual", ColorRange.Gradual);
            var colorRangeStart = ColorRange.Start;
            Shader.SetVector3("basecolor", new Vector3(colorRangeStart.R, colorRangeStart.G, colorRangeStart.B));
        }

        public override void ApplyDirective(RenderDirective directive)
        {
            base.ApplyDirective(directive);
            if (directive is RenderDirective3D directive3D)
            {
                var identity = Matrix4.Identity;
                identity *= directive3D.Projection;
                identity *= directive3D.View;
                identity *= ModelPosition;
                Shader.SetMatrix4("transform", identity);
            }
        }
    }
}