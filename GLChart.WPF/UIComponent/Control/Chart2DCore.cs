using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render.CollisionDetection;
using GLChart.WPF.Render.Renderer;
using GLChart.WPF.UIComponent.Axis;
using GLChart.WPF.UIComponent.Interaction;
using OpenTkWPFHost.Control;
using OpenTkWPFHost.Core;

namespace GLChart.WPF.UIComponent.Control;

/// <summary>
/// 融合了Renderer和Coordinate2D的基础图
/// </summary>
// [TemplatePart(Name = ToolTipName, Type = typeof(ToolTip))]
[TemplatePart(Name = CoordinateElementName, Type = typeof(Coordinate2D))]
[TemplatePart(Name = ThreadOpenTkControl, Type = typeof(BitmapOpenTkControl))]
[TemplatePart(Name = SelectScaleElement, Type = typeof(MouseInteractionElement))]
[TemplatePart(Name = Coordinate2DRendererName, Type = typeof(Coordinate2DRenderer))]
public abstract class Chart2DCore : System.Windows.Controls.Control
{
    private const string CoordinateElementName = "Coordinate";

    private const string SelectScaleElement = "SelectScaleElement";

    private const string ToolTipName = "ToolTip";

    private const string ThreadOpenTkControl = "OpenTkControl";

    private const string Coordinate2DRendererName = "Coordinate2DRenderer";

    static Chart2DCore()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Chart2DCore),
            new FrameworkPropertyMetadata(typeof(Chart2DCore)));
    }

    public static readonly DependencyProperty ToolTipTemplateProperty = DependencyProperty.Register(
        nameof(ToolTipTemplate), typeof(DataTemplate), typeof(Chart2DCore),
        new PropertyMetadata(default(DataTemplate)));

    public DataTemplate ToolTipTemplate
    {
        get => (DataTemplate)GetValue(ToolTipTemplateProperty);
        set => SetValue(ToolTipTemplateProperty, value);
    }

    #region coordinate

    public static readonly DependencyProperty AxisXOptionProperty = DependencyProperty.Register(
        nameof(AxisXOption), typeof(AxisXOption), typeof(Chart2DCore),
        new FrameworkPropertyMetadata(default));

    public AxisXOption AxisXOption
    {
        get => (AxisXOption)GetValue(AxisXOptionProperty);
        set => SetValue(AxisXOptionProperty, value);
    }

    public static readonly DependencyProperty AxisYOptionProperty = DependencyProperty.Register(
        nameof(AxisYOption), typeof(AxisYOption), typeof(Chart2DCore),
        new FrameworkPropertyMetadata(default));

    public AxisYOption AxisYOption
    {
        get => (AxisYOption)GetValue(AxisYOptionProperty);
        set => SetValue(AxisYOptionProperty, value);
    }

    #endregion

    #region render

    protected BitmapOpenTkControl? RenderControl;

    public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
        nameof(BackgroundColor), typeof(Color), typeof(Chart2DCore),
        new PropertyMetadata(Colors.White));

    public Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public static readonly DependencyProperty IsShowFpsProperty = DependencyProperty.Register(
        nameof(IsShowFps), typeof(bool), typeof(Chart2DCore), new PropertyMetadata(default(bool)));

    public bool IsShowFps
    {
        get => (bool)GetValue(IsShowFpsProperty);
        set => SetValue(IsShowFpsProperty, value);
    }

    protected Coordinate2DRenderer? CoordinateRenderer;

    #endregion

    private MouseInteractionElement? _scaleElement;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _scaleElement = GetTemplateChild(SelectScaleElement) as MouseInteractionElement;
        _scaleElement!.MouseMove += OnScaleElementMouseMove;
        _scaleElement.MouseLeave += OnRenderControlMouseLeave;
        /*_toolTip = GetTemplateChild(ToolTipName) as ToolTip;
        _toolTip!.PlacementTarget = this;*/
        CoordinateRenderer = GetTemplateChild(Coordinate2DRendererName) as Coordinate2DRenderer;
        RenderControl = GetTemplateChild(ThreadOpenTkControl) as BitmapOpenTkControl;
        RenderControl!.RenderErrorReceived += OpenTkControlOnRenderErrorReceived;
        RenderControl.OpenGlErrorReceived += OpenTkControlOnOpenGlErrorReceived;
        // RenderControl.MouseMove;
        _toolTip = RenderControl.ToolTip as ToolTip;
        _toolTip.PlacementTarget = RenderControl;
    }

    protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
    {
        if (_scaleElement != null)
        {
            _scaleElement.MouseMove -= OnScaleElementMouseMove;
            _scaleElement.MouseLeave -= OnRenderControlMouseLeave;
        }

        if (RenderControl != null)
        {
            RenderControl.RenderErrorReceived -= OpenTkControlOnRenderErrorReceived;
            RenderControl.OpenGlErrorReceived -= OpenTkControlOnOpenGlErrorReceived;
        }

        base.OnTemplateChanged(oldTemplate, newTemplate);
    }

    #region render event handler

    private static void OpenTkControlOnOpenGlErrorReceived(object? sender, OpenGlErrorArgs e)
    {
        Trace.WriteLine(e.ToString());
    }

    private static void OpenTkControlOnRenderErrorReceived(object? sender, RenderErrorArgs e)
    {
        Trace.WriteLine($"{e.Exception.Message}");
    }

    #endregion

    #region interaction

    private ToolTip? _toolTip;

    protected virtual void OnRenderControlMouseLeave(object sender, MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_toolTip?.IsOpen == true)
        {
            _toolTip.IsOpen = false;
        }
    }

    protected readonly CollisionGrid2D CollisionGrid = new CollisionGrid2D();

    private IGeometry? _toolTipNode;

    protected readonly List<ISeries2D> Series2Ds = new List<ISeries2D>(5);

    protected virtual void OnScaleElementMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(RenderControl); //像素
        var coordinateRegion = this._scaleElement!.ActualRegion;
        var winToGlMapping =
            new WindowsGlCoordinateMapping(coordinateRegion, new Rect(RenderControl!.RenderSize));
        if (e.LeftButton != MouseButtonState.Pressed
            && e.RightButton != MouseButtonState.Pressed)
        {
            if (_toolTip == null)
            {
                return;
            }

            var mapGlPoint = winToGlMapping.GetGlPointByWindowsPoint(position);
            var xDistance = winToGlMapping.XScaleRatio * 10;
            var yDistance = winToGlMapping.YScaleRatio * 10;
            var ellipse = new MouseCollisionEllipse(mapGlPoint, (float)xDistance, (float)yDistance);
            if (CollisionGrid.TrySearch(ellipse, out var geometry, out var layer))
            {
                if (_toolTipNode?.Equals(geometry) != true && geometry != null)
                {
                    _toolTipNode = geometry;
                    // var winPoint = winToGlMapping.GetWindowsPointByGlPoint(geometry.Point);
                    var seriesItem = Series2Ds.First(item =>
                        item.CollisionLayer.Equals(layer));
                    _toolTip.HorizontalOffset = position.X + 10;
                    _toolTip.VerticalOffset = position.Y + 10;
                    _toolTip.Content = new MouseHoverNodeData(seriesItem.Color, geometry,
                        seriesItem.Title);
                }

                if (!_toolTip.IsOpen)
                {
                    _toolTip.IsOpen = true;
                }

                return;
            }
        }

        if (_toolTip?.IsOpen == true)
        {
            _toolTip.IsOpen = false;
        }
    }

    #endregion
}