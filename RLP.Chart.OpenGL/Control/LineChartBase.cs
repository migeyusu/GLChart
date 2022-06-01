#define SimpleLine

// #define AdvancedLine
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using OpenTK.Graphics;
using OpenTkWPFHost.Configuration;
using OpenTkWPFHost.Control;
using OpenTkWPFHost.Core;
using RLP.Chart.Interface;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.Control
{
    /// <summary>
    /// 只具有显示能力的图形
    /// </summary>
    [TemplatePart(Name = ThreadOpenTkControl, Type = typeof(ThreadOpenTkControl))]
    public class LineChartBase : System.Windows.Controls.Control, ILineChart
    {
        public static Dispatcher AppDispatcher => DispatcherLazy.Value;

        private static readonly Lazy<Dispatcher> DispatcherLazy = new Lazy<Dispatcher>(
            () => Application.Current.Dispatcher,
            LazyThreadSafetyMode.ExecutionAndPublication);

        public const string ThreadOpenTkControl = "ThreadOpenTkControl";

        public static readonly DependencyProperty IsShowFpsProperty = DependencyProperty.Register(
            "IsShowFps", typeof(bool), typeof(LineChartBase), new PropertyMetadata(true));

        public bool IsShowFps
        {
            get { return (bool)GetValue(IsShowFpsProperty); }
            set { SetValue(IsShowFpsProperty, value); }
        }

        public static readonly DependencyProperty AutoYAxisProperty = DependencyProperty.Register(
            "AutoYAxis", typeof(bool), typeof(LineChartBase), new PropertyMetadata(true));

        public virtual bool AutoYAxis
        {
            get { return (bool)GetValue(AutoYAxisProperty); }
            set { SetValue(AutoYAxisProperty, value); }
        }

        //ActualRegion : SettingRegion
        public virtual Region2D DisplayRegion
        {
            get { return AutoYAxis ? ActualRegion : SettingRegion; }
            set { SetValue(SettingRegionProperty, value); }
        }

        public static readonly DependencyProperty GlSettingsProperty = DependencyProperty.Register(
            "GlSettings", typeof(GLSettings), typeof(LineChartBase),
            new PropertyMetadata(new GLSettings()
            {
                GraphicsContextFlags = GraphicsContextFlags.Offscreen,
                GraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 4)
            }));

        public GLSettings GlSettings
        {
            get { return (GLSettings)GetValue(GlSettingsProperty); }
            set { SetValue(GlSettingsProperty, value); }
        }

        public static readonly DependencyProperty SettingRegionProperty = DependencyProperty.Register(
            "SettingRegion", typeof(Region2D), typeof(LineChartBase),
            new PropertyMetadata(default(Region2D)));

        public Region2D SettingRegion
        {
            get { return (Region2D)GetValue(SettingRegionProperty); }
            set { SetValue(SettingRegionProperty, value); }
        }

        public static readonly DependencyProperty ActualRegionProperty = DependencyProperty.Register(
            "ActualRegion", typeof(Region2D), typeof(LineChartBase),
            new PropertyMetadata(default(Region2D)));

        /// <summary>
        /// 当前chart实际坐标
        /// </summary>
        public Region2D ActualRegion
        {
            get { return (Region2D)GetValue(ActualRegionProperty); }
            set { SetValue(ActualRegionProperty, value); }
        }

        public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register(
            "LineThickness", typeof(double), typeof(LineChartBase), new PropertyMetadata(1d));

        public double LineThickness
        {
            get { return (double)GetValue(LineThicknessProperty); }
            set { SetValue(LineThicknessProperty, value); }
        }

        public static readonly DependencyProperty MaxLineThicknessProperty = DependencyProperty.Register(
            "MaxLineThickness", typeof(double), typeof(LineChartBase), new PropertyMetadata(default(double)));

        public double MaxLineThickness
        {
            get { return (double)GetValue(MaxLineThicknessProperty); }
            set { SetValue(MaxLineThicknessProperty, value); }
        }

        public static readonly DependencyProperty MinLineThicknessProperty = DependencyProperty.Register(
            "MinLineThickness", typeof(double), typeof(LineChartBase), new PropertyMetadata(default(double)));

        public double MinLineThickness
        {
            get { return (double)GetValue(MinLineThicknessProperty); }
            set { SetValue(MinLineThicknessProperty, value); }
        }

        public virtual IReadOnlyList<ILineSeries> SeriesItems => new ReadOnlyCollection<SeriesItemLight>(_items);


#if SimpleLine
                protected SimpleLineSeriesRenderer LineSeriesRenderer =
            new SimpleLineSeriesRenderer(new Shader("Shaders/LineShader/shader.vert", "Shaders/LineShader/shader.frag"));
#else
        protected AdvancedLineSeriesRenderer LineSeriesRenderer =
            new AdvancedLineSeriesRenderer(new Shader("Shaders/AdvancedLineShader/shader.vert",
                "Shaders/AdvancedLineShader/shader.frag"));
#endif
        protected Coordinate2DRenderer CoordinateRenderer;

        protected ThreadOpenTkControl OpenTkControl;

        static LineChartBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineChartBase),
                new FrameworkPropertyMetadata(typeof(LineChartBase)));
        }

        public LineChartBase()
        {
            CoordinateRenderer = new Coordinate2DRenderer(new BaseRenderer[] { LineSeriesRenderer });
            DependencyPropertyDescriptor.FromProperty(SettingRegionProperty, typeof(LineChartBase))
                .AddValueChanged(this, SettingRegionChangedHandler);
            DependencyPropertyDescriptor.FromProperty(LineThicknessProperty, typeof(LineChartBase))
                .AddValueChanged(this, LineThicknessChangedHandler);
            DependencyPropertyDescriptor.FromProperty(AutoYAxisProperty, typeof(LineChartBase))
                .AddValueChanged(this, AutoYAxisChanged_Handler);
            CoordinateRenderer.AutoYAxisApex = (bool)AutoYAxisProperty.DefaultMetadata.DefaultValue;
        }

        private void AutoYAxisChanged_Handler(object sender, EventArgs e)
        {
            CoordinateRenderer.AutoYAxisApex = this.AutoYAxis;
        }

/*当需要同步依赖属性和独立变量时，使用观察者，倾向于在load或apply template后再绑定以防止未加载的空控件，同时必须初始化值*/

        protected virtual void LineThicknessChangedHandler(object sender, EventArgs e)
        {
            LineSeriesRenderer.LineThickness = (float)this.LineThickness;
        }

        protected virtual void SettingRegionChangedHandler(object sender, EventArgs e)
        {
            var region = this.SettingRegion;
            this.CoordinateRenderer.TargetRegion = region;
            this.ActualRegion = region; //直接拷贝数据
            // Debug.WriteLine(region.ToString());
        }

        protected virtual void SeriesRendererHost_AutoAxisYCompleted(Region2D region)
        {
            // AppDispatcher.Invoke(() => { OpenTkControl.IsRenderContinuously = false; });
            AppDispatcher.InvokeAsync(() => { this.ActualRegion = region; });
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            OpenTkControl = GetTemplateChild(ThreadOpenTkControl) as ThreadOpenTkControl;
            CoordinateRenderer.ActualRegionChanged += SeriesRendererHost_AutoAxisYCompleted;
            OpenTkControl.Renderer = CoordinateRenderer;
            OpenTkControl.RenderErrorReceived += OpenTkControlOnRenderErrorReceived;
            OpenTkControl.OpenGlErrorReceived += OpenTkControlOnOpenGlErrorReceived;
        }

        private void OpenTkControlOnOpenGlErrorReceived(object sender, OpenGlErrorArgs e)
        {
            Debug.WriteLine(e.ToString());
        }

        private void OpenTkControlOnRenderErrorReceived(object sender, RenderErrorArgs e)
        {
            Debug.WriteLine($"{e.Phase}:{e.Exception.Message}");
        }

        public virtual void AttachWindow(Window hostWindow)
        {
            this.OpenTkControl.Start(hostWindow);
        }

        public virtual void DetachWindow()
        {
            this.OpenTkControl.Close();
        }

        private readonly List<SeriesItemLight> _items = new List<SeriesItemLight>(5);

        public virtual ILineSeries NewSeries()
        {
            return new SeriesItemLight();
        }

        public virtual void Add(ILineSeries item)
        {
            if (item is SeriesItemLight seriesItemLight)
            {
                this._items.Add(seriesItemLight);
                this.LineSeriesRenderer.Add(seriesItemLight.Renderer);
            }
        }

        public virtual void Remove(ILineSeries seriesItem)
        {
            var seriesItemLight = _items.Find(light => light.Id == seriesItem.Id);
            this._items.Remove(seriesItemLight);
            this.LineSeriesRenderer.Remove(seriesItemLight.Renderer);
        }

        public virtual void Clear()
        {
            this.LineSeriesRenderer.Clear();
            this._items.Clear();
        }

        public class SeriesItemLight : ILineSeries
        {
            public Guid Id { get; } = Guid.NewGuid();
            public string Title { get; set; }

            public bool Visible
            {
                get => Renderer.RenderEnable;
                set => Renderer.RenderEnable = value;
            }

            public Color LineColor
            {
                get { return Renderer.LineColor; }
                set { Renderer.LineColor = value; }
            }

            public int PointCountLimit
            {
                get => Renderer.PointCountLimit;
                set => Renderer.PointCountLimit = value;
            }

            internal SimpleLineRenderer Renderer { get; }

            public SeriesItemLight()
            {
                this.Renderer = new SimpleLineRenderer();
            }

            public virtual void AddGeometry(IPoint2D point)
            {
                this.Renderer.AddGeometry(point);
            }

            public virtual void AddGeometries(IList<IPoint2D> points)
            {
                this.Renderer.AddGeometries(points);
            }

            public virtual void ResetWith(IList<IPoint2D> geometries)
            {
                this.Renderer.ResetWith(geometries);
            }

            public virtual void ResetWith(IPoint2D geometry)
            {
                this.Renderer.ResetWith(geometry);
            }

            public virtual void Clear()
            {
                this.Renderer.Clear();
            }

            protected bool Equals(SeriesItemLight other)
            {
                return Id.Equals(other.Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((SeriesItemLight)obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }
    }
}