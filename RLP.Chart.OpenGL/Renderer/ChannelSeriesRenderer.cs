using System;
using System.Linq;
using OpenTK.Graphics;
using OpenTkWPFHost.Core;

namespace RLP.Chart.OpenGL.Renderer
{
    public class ChannelSeriesRenderer : SeriesShaderRenderer<ChannelRenderer>
    {
        public ChannelSeriesRenderer(Shader shader) : base(shader)
        {
        }

    }
}