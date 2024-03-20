using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render;
using GLChart.WPF.Render.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Abstraction;
using OpenTkWPFHost.Core;

namespace GLChart.Samples.Charts;

public class Coordinate2DRendererView : FrameworkElement, IRenderer
{
    public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
        nameof(BackgroundColor), typeof(Color), typeof(Coordinate2DRendererView),
        new PropertyMetadata(Colors.White, ColorChangedCallback));

    public Color BackgroundColor
    {
        get { return (Color)GetValue(BackgroundColorProperty); }
        set { SetValue(BackgroundColorProperty, value); }
    }

    private static void ColorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Coordinate2DRendererView renderer)
        {
            var value = (Color)e.NewValue;
            renderer._backgoundColorValue = new Color4(value.R, value.G, value.B, value.A);
        }
    }

    private Color4 _backgoundColorValue;

    private Coordinate2DRenderer? _masterRenderer;

    public ScrollRange DefaultYRange { get; set; } = new ScrollRange(0, 100);

    public static readonly DependencyProperty RangeProperty = DependencyProperty.Register(
        nameof(Range), typeof(ScrollRange), typeof(Coordinate2DRendererView),
        new PropertyMetadata(ScrollRange.Empty, RangeChangedCallback));

    private ScrollRange _rangeValue;

    private static void RangeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Coordinate2DRendererView view)
        {
            view._rangeValue = (ScrollRange)e.NewValue;
        }
    }

    public ScrollRange Range
    {
        get { return (ScrollRange)GetValue(RangeProperty); }
        set { SetValue(RangeProperty, value); }
    }

    public Coordinate2DRendererView()
    {
        var value = (Color)BackgroundColorProperty.DefaultMetadata.DefaultValue;
        this._backgoundColorValue = new Color4(value.R, value.G, value.B, value.A);
    }

    public void Bind(Coordinate2DRenderer renderer)
    {
        this._masterRenderer = renderer;
        renderer.NewRenderRequest += RendererOnNewRenderRequest;
    }

    private bool _masterRenderEnable = false;

    private void RendererOnNewRenderRequest()
    {
        _masterRenderEnable = true;
    }

    private ScrollRange _lastRange = ScrollRange.Empty;

    private Matrix4 _transformMatrix = Matrix4.Zero;

    public Region2D RenderingRegion2D
    {
        get => _renderingRegion2D;
        set
        {
            _renderingRegion2D = value;
            _transformMatrix = value.ToTransformMatrix();
        }
    }

    private Region2D _renderingRegion2D = new Region2D(100, 0, 0, 0);

    public void Initialize(IGraphicsContext context)
    {
        if (IsInitialized)
        {
            return;
        }

        IsInitialized = true;
    }

    public bool PreviewRender()
    {
        if (_masterRenderer == null)
        {
            return false;
        }
        
        if (_rangeValue.IsEmpty())
        {
            return false;
        }

        var enable = false;
        if (!_lastRange.Equals(_rangeValue))
        {
            enable = true;
            _lastRange = _rangeValue;
            RenderingRegion2D = RenderingRegion2D.ChangeXRange(_rangeValue);
        }

        enable = enable || _isHeightAdapting || _masterRenderEnable;
        _masterRenderEnable = false;
        return enable;
    }

    /// <summary>
    /// 指示是否正在自适应地调整高度
    /// </summary>
    private volatile bool _isHeightAdapting = false;

    public void Render(GlRenderEventArgs args)
    {
        GL.ClearColor(_backgoundColorValue);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        var isHeightAdapting = _isHeightAdapting;
        RenderingRegion2D = _masterRenderer!.RenderInstance(args, ref isHeightAdapting, true, _transformMatrix,
            RenderingRegion2D, DefaultYRange);
        this._isHeightAdapting = isHeightAdapting;
    }

    public void Resize(PixelSize size)
    {
        GL.Viewport(0, 0, size.Width, size.Height);
    }

    public void Uninitialize()
    {
    }

    public new bool IsInitialized { get; private set; }
}