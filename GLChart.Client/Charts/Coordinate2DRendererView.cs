using GLChart.WPF.Base;
using GLChart.WPF.Render.Renderer;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Abstraction;
using OpenTkWPFHost.Core;

namespace GLChart.Samples.Charts;

public class Coordinate2DRendererView : IRenderer
{
    private Coordinate2DRenderer? _renderer;

    public ScrollRange Range { get; set; }
    
    public Coordinate2DRendererView()
    {
    }

    public void Bind(Coordinate2DRenderer renderer)
    {
        this._renderer = renderer;
    }

    public void Initialize(IGraphicsContext context)
    {
    }

    public bool PreviewRender()
    {
        return true;
    }

    public void Render(GlRenderEventArgs args)
    {
    }

    public void Resize(PixelSize size)
    {
        throw new System.NotImplementedException();
    }

    public void Uninitialize()
    {
        throw new System.NotImplementedException();
    }

    public bool IsInitialized { get; }
}