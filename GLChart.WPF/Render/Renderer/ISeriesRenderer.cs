using System;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Core;

namespace GLChart.WPF.Render.Renderer
{
    /// <summary>
    /// 系列渲染器
    /// </summary>
    public interface ISeriesRenderer : IRendererItem
    {
        /// <summary>
        /// 是否有准备的渲染器
        /// </summary>
        /// <returns></returns>
        bool AnyReadyRenders();
    }
}