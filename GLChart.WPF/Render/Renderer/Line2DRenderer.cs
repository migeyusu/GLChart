using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render.Allocation;
using GLChart.WPF.Render.CollisionDetection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Core;

namespace GLChart.WPF.Render.Renderer;

/// <summary>
/// todo:线条渲染器
/// </summary>
public class Line2DRenderer : IShaderRendererItem, ILine2D
{
    public Guid Id { get; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    private volatile bool _renderEnable = true;

    public bool RenderEnable
    {
        get => _renderEnable;
        set => _renderEnable = value;
    }

    public bool IsInitialized
    {
        get => _isInitialized;
        protected set => _isInitialized = value;
    }

    protected Color4 Color4 = Color4.Blue;

    private Color _color;

    public Color Color
    {
        get { return _color; }
        set
        {
            if (Equals(_color, value))
            {
                return;
            }

            _color = value;
            Color4 = new Color4(value.R, value.G, value.B, value.A);
        }
    }

    public ICollision2DLayer CollisionLayer => CollisionPoint2D;

    /// <summary>
    /// 线宽
    /// </summary>
    public float Thickness { get; set; } = 2;

    protected const int SizeFloat = sizeof(float);

    private volatile bool _isInitialized;

    protected readonly ICollisionPoint2D CollisionPoint2D;

    private readonly ModelRingBuffer<IPoint2D, float> _pointsBuffer = new ModelRingBuffer<IPoint2D, float>();

    /// <summary>
    /// 
    /// </summary>
    internal Line2DRenderer(ICollisionPoint2D collisionPoint2D)
    {
        CollisionPoint2D = collisionPoint2D;
    }

    protected Shader? Shader;

    /*v1 elementdraw 使用固定大小多块缓冲扩展
     v2
     */

    public virtual void Initialize(IGraphicsContext context)
    {
    }

    public virtual bool PreviewRender()
    {
        return true;
    }

    public virtual void Render(GlRenderEventArgs args)
    {
    }

    public virtual void ApplyDirective(RenderDirective directive)
    {
        //no implement
    }

    public virtual void BindShader(Shader shader)
    {
        this.Shader = shader;
    }

    public virtual void Resize(PixelSize size)
    {
        //no implement
    }

    /// <summary>
    /// 冲洗之前操作到设备缓冲，在opengl上下文调用
    /// </summary>
    /// <returns>true:更新了设备缓冲；false：未更新</returns>
    public virtual void Add(IPoint2D point)
    {
    }

    public virtual void AddRange(IList<IPoint2D> points)
    {
    }

    public virtual void Remove(IPoint2D geometry)
    {
    }

    public virtual void RemoveAt(int index)
    {
    }

    public virtual void Insert(int index, IPoint2D geometry)
    {
    }

    public virtual void ResetWith(IList<IPoint2D> geometries)
    {
    }

    public virtual void ResetWith(IPoint2D geometry)
    {
    }

    public virtual void Clear()
    {
    }

    public virtual void Uninitialize()
    {
        IsInitialized = false;
    }

    private bool Equals(IRendererItem other)
    {
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((IRendererItem)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}