using System;
using System.Collections.Generic;
using System.Drawing;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.Control
{
    /// <summary>
    /// 基础的线系列
    /// </summary>
    public class LineSeriesBase : SimpleLineRenderer
    {
        public string Title { get; set; }

        public bool Visible
        {
            get => this.RenderEnable;
            set => this.RenderEnable = value;
        }
    }
}