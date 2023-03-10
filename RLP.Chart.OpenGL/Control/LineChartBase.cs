#define SimpleLine

// #define AdvancedLine
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
    public class LineChartBase : System.Windows.Controls.Control, ISeriesChart<LineSeriesBase>
    {
        public static Dispatcher AppDispatcher => DispatcherLazy.Value;

        private static readonly Lazy<Dispatcher> DispatcherLazy = new Lazy<Dispatcher>(
            () => Application.Current.Dispatcher,
            LazyThreadSafetyMode.ExecutionAndPublication);

        public const string ThreadOpenTkControl = "ThreadOpenTkControl";

        public bool IsShowFps
        {
            get => _isShowFps;
            set
            {
                _isShowFps = value;
                if (OpenTkControl != null)
                {
                    OpenTkControl.IsShowFps = value;
                }
            }
        }

        public static readonly DependencyProperty AutoYAxisProperty = DependencyProperty.Register(
            "AutoYAxis", typeof(bool), typeof(LineChartBase), new PropertyMetadata(true));

        public virtual bool AutoYAxisEnable
        {
            get { return (bool)GetValue(AutoYAxisProperty); }
            set { SetValue(AutoYAxisProperty, value); }
        }

        //ActualRegion : SettingRegion
        public virtual Region2D DisplayRegion
        {
            get { return CoordinateRenderer.AutoYAxisEnable ? ActualRegion : SettingRegion; }
            set { SetValue(SettingRegionProperty, value); }
        }

        /// <summary>
        /// gl 设置
        /// </summary>
        public GLSettings GlSettings
        {
            get => _glSettings;
            set
            {
                _glSettings = value;
                if (OpenTkControl != null)
                {
                    OpenTkControl.GlSettings = value;
                }
            }
        }

        public static readonly DependencyProperty SettingRegionProperty = DependencyProperty.Register(
            "SettingRegion", typeof(Region2D), typeof(LineChartBase),
            new PropertyMetadata(default(Region2D)));

        /// <summary>
        /// 设置的视域
        /// </summary>
        public Region2D SettingRegion
        {
            get { return (Region2D)GetValue(SettingRegionProperty); }
            set { SetValue(SettingRegionProperty, value); }
        }

        public static readonly DependencyProperty ActualRegionProperty = DependencyProperty.Register(
            "ActualRegion", typeof(Region2D), typeof(LineChartBase),
            new PropertyMetadata(default(Region2D)));

        /// <summary>
        /// 当前实际视域
        /// </summary>
        public Region2D ActualRegion
        {
            get { return (Region2D)GetValue(ActualRegionProperty); }
            set { SetValue(ActualRegionProperty, value); }
        }

        public virtual IReadOnlyList<LineSeriesBase> SeriesItems =>
            new ReadOnlyCollection<LineSeriesBase>(LineSeriesRenderer.Cast<LineSeriesBase>().ToArray());


#if SimpleLine
        protected SimpleLineSeriesRenderer LineSeriesRenderer =
            new SimpleLineSeriesRenderer(new Shader("Shaders/LineShader/shader.vert",
                "Shaders/LineShader/shader.frag"));
#else
        protected AdvancedLineSeriesRenderer LineSeriesRenderer =
            new AdvancedLineSeriesRenderer(new Shader("Shaders/AdvancedLineShader/shader.vert",
                "Shaders/AdvancedLineShader/shader.frag"));
#endif
        protected AutoHeight2DRenderer CoordinateRenderer;

        protected ThreadOpenTkControl OpenTkControl;

        static LineChartBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineChartBase),
                new FrameworkPropertyMetadata(typeof(LineChartBase)));
        }

        public LineChartBase()
        {
            CoordinateRenderer = new AutoHeight2DRenderer(new BaseRenderer[] { LineSeriesRenderer });
            DependencyPropertyDescriptor.FromProperty(SettingRegionProperty, typeof(LineChartBase))
                .AddValueChanged(this, SettingRegionChangedHandler);
            DependencyPropertyDescriptor.FromProperty(AutoYAxisProperty, typeof(LineChartBase))
                .AddValueChanged(this, AutoYAxisChanged_Handler);
            CoordinateRenderer.AutoYAxisEnable = (bool)AutoYAxisProperty.DefaultMetadata.DefaultValue;
        }

        private void AutoYAxisChanged_Handler(object sender, EventArgs e)
        {
            CoordinateRenderer.AutoYAxisEnable = this.AutoYAxisEnable;
        }

/*当需要同步依赖属性和独立变量时，使用观察者，倾向于在load或apply template后再绑定以防止未加载的空控件，同时必须初始化值*/

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
            OpenTkControl.GlSettings = this.GlSettings;
            CoordinateRenderer.ActualRegionChanged += SeriesRendererHost_AutoAxisYCompleted;
            OpenTkControl.Renderer = CoordinateRenderer;
            OpenTkControl.RenderErrorReceived += OpenTkControlOnRenderErrorReceived;
            OpenTkControl.OpenGlErrorReceived += OpenTkControlOnOpenGlErrorReceived;
        }

        private void OpenTkControlOnOpenGlErrorReceived(object sender, OpenGlErrorArgs e)
        {
            Trace.WriteLine(e.ToString());
        }

        private void OpenTkControlOnRenderErrorReceived(object sender, RenderErrorArgs e)
        {
            Trace.WriteLine($"{e.Phase}:{e.Exception.Message}");
        }

        public virtual void AttachWindow(Window hostWindow)
        {
            this.OpenTkControl.Start(hostWindow);
        }

        public virtual void DetachWindow()
        {
            this.OpenTkControl.Close();
        }

        private GLSettings _glSettings = new GLSettings()
        {
            GraphicsContextFlags = GraphicsContextFlags.Offscreen,
            GraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 4)
        };

        private bool _isShowFps;

        public virtual LineSeriesBase NewSeries()
        {
            return new LineSeriesBase();
        }

        public virtual void Add(LineSeriesBase item)
        {
            this.LineSeriesRenderer.Add(item);
        }

        public virtual void Remove(LineSeriesBase seriesItem)
        {
            this.LineSeriesRenderer.Remove(seriesItem);
        }

        public virtual void Clear()
        {
            this.LineSeriesRenderer.Clear();
        }
    }
}