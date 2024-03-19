﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using GLChart.WPF.Base;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Abstraction;
using OpenTkWPFHost.Core;

namespace GLChart.WPF.Render.Renderer
{
    /*2021.12.22：
     已知问题：不能设置零一下的Y轴起点*/

    /// <summary>
    /// 基于2d 坐标轴的渲染器；
    /// 能够自适应内容高度
    /// </summary>
    public class Coordinate2DRenderer : DependencyObject, IRenderer
    {
        /// <summary>
        /// 点采样函数，渲染大量点位时允许只渲染部分点，todo:未实现
        /// </summary>
        public Func<int, ScrollRange>? SamplingFunction { get; set; }


        public static readonly DependencyProperty AutoYAxisEnableProperty = DependencyProperty.Register(
            nameof(AutoYAxisEnable), typeof(bool), typeof(Coordinate2DRenderer),
            new PropertyMetadata(true, (AutoYAxisEnableChangedCallback)));

        private static void AutoYAxisEnableChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Coordinate2DRenderer coordinate2DRenderer)
            {
                coordinate2DRenderer._autoYAxisEnableValue = (bool)e.NewValue;
            }
        }

        public bool AutoYAxisEnable
        {
            get { return (bool)GetValue(AutoYAxisEnableProperty); }
            set { SetValue(AutoYAxisEnableProperty, value); }
        }

        private bool _autoYAxisEnableValue;

        public static readonly DependencyProperty DefaultAxisYRangeProperty = DependencyProperty.Register(
            nameof(DefaultAxisYRange), typeof(ScrollRange), typeof(Coordinate2DRenderer),
            new PropertyMetadata(new ScrollRange(0, 100), (DefaultAxisYRangeChangedCallback)));

        private static void DefaultAxisYRangeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Coordinate2DRenderer renderer)
            {
                renderer._defaultAxisYRangeValue = (ScrollRange)e.NewValue;
            }
        }

        public ScrollRange DefaultAxisYRange
        {
            get { return (ScrollRange)GetValue(DefaultAxisYRangeProperty); }
            set { SetValue(DefaultAxisYRangeProperty, value); }
        }

        /// <summary>
        /// 自适应Y轴的默认区间，当界面内没有元素时显示该区间
        /// </summary>
        private ScrollRange _defaultAxisYRangeValue;

        public static readonly DependencyProperty ActualYRangeProperty = DependencyProperty.Register(
            nameof(ActualYRange), typeof(ScrollRange), typeof(Coordinate2DRenderer),
            new PropertyMetadata(default(ScrollRange)));

        public ScrollRange ActualYRange
        {
            get { return (ScrollRange)GetValue(ActualYRangeProperty); }
            set { SetValue(ActualYRangeProperty, value); }
        }

        public static readonly DependencyProperty TargetXRangeProperty = DependencyProperty.Register(
            nameof(TargetXRange), typeof(ScrollRange), typeof(Coordinate2DRenderer),
            new PropertyMetadata(default(ScrollRange), (TargetXRangeChangedCallback)));

        private static void TargetXRangeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Coordinate2DRenderer renderer)
            {
                var scrollRange = (ScrollRange)e.NewValue;
                renderer._targetRegion2D = renderer._targetRegion2D.ChangeXRange(scrollRange);
            }
        }

        public ScrollRange TargetXRange
        {
            get { return (ScrollRange)GetValue(TargetXRangeProperty); }
            set { SetValue(TargetXRangeProperty, value); }
        }

        public static readonly DependencyProperty TargetYRangeProperty = DependencyProperty.Register(
            nameof(TargetYRange), typeof(ScrollRange), typeof(Coordinate2DRenderer),
            new PropertyMetadata(default(ScrollRange), (TargetYRangeChangedCallback)));

        private static void TargetYRangeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Coordinate2DRenderer renderer)
            {
                var scrollRange = (ScrollRange)e.NewValue;
                renderer._targetRegion2D = renderer._targetRegion2D.ChangeYRange(scrollRange);
            }
        }

        public ScrollRange TargetYRange
        {
            get { return (ScrollRange)GetValue(TargetYRangeProperty); }
            set { SetValue(TargetYRangeProperty, value); }
        }

        private Region2D _targetRegion2D;

        private Matrix4 _tempTransform = Matrix4.Identity;

        private Region2D _renderingRegion = default;

        /// <summary>
        /// 实际渲染区域
        /// </summary>
        private Region2D RenderingRegion
        {
            get => _renderingRegion;
            set
            {
                this._renderingRegion = value;
                Dispatcher.InvokeAsync(() => { this.ActualYRange = value.YRange; });
                this._tempTransform = GetTransform(value);
            }
        }

        /// <summary>
        /// 渲染快照
        /// </summary>
        private IList<ISeriesRenderer> _rendererSeriesSnapList = new List<ISeriesRenderer>();

        /// <summary>
        /// 基于ndc的Y轴（-1-1）进行划分的块数量
        /// </summary>
        private const int NdcYAxisSpacialSplitCount = 200;

        /// <summary>
        /// ndc ssbo 长度
        /// </summary>
        private const int YAxisCastSSBOLength = 300;

        /// <summary>
        /// 指示是否正在自适应地调整高度
        /// </summary>
        private volatile bool _isHeightAdapting;

        /// <summary>
        /// 基于ndc的y轴投影计量缓冲
        /// </summary>
        private int _yAxisCastSSBO;

        private readonly int[] _yAxisRaster = new int[YAxisCastSSBOLength];

        private readonly int[] _emptySsboBuffer = new int[YAxisCastSSBOLength];

        public const float AutoAxisYContentRatio = 0.8f;

        public const float AutoAxisYContentRatioAccuracy = 0.05f;

        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
            nameof(BackgroundColor), typeof(Color), typeof(Coordinate2DRenderer),
            new PropertyMetadata(Colors.White, (ColorChangedCallback)));

        private static void ColorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Coordinate2DRenderer renderer)
            {
                var value = (Color)e.NewValue;
                renderer._backgroundColor4 = new Color4(value.R, value.G, value.B, value.A);
            }
        }

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        private Color4 _backgroundColor4;

        public IReadOnlyCollection<ISeriesRenderer> Series =>
            new ReadOnlyCollection<ISeriesRenderer>(RenderSeriesCollection);

        protected readonly IList<ISeriesRenderer> RenderSeriesCollection = new List<ISeriesRenderer>();

        public Coordinate2DRenderer()
        {
            this._autoYAxisEnableValue = (bool)AutoYAxisEnableProperty.DefaultMetadata.DefaultValue;
            this._defaultAxisYRangeValue = (ScrollRange)DefaultAxisYRangeProperty.DefaultMetadata.DefaultValue;
            var value = (Color)BackgroundColorProperty.DefaultMetadata.DefaultValue;
            this._backgroundColor4 = new Color4(value.R, value.G, value.B, value.A);
        }

        private static Matrix4 GetTransform(Region2D value)
        {
            var transform = Matrix4.Identity;
            var xScale = 2f / (value.Right - value.Left);
            var yScale = 2f / (value.Top - value.Bottom);
            transform *= Matrix4.CreateScale((float)xScale,
                (float)yScale, 0);
            var xStart = xScale * value.Left;
            var yStart = yScale * value.Bottom;
            transform *= Matrix4.CreateTranslation((float)(-1 - xStart), (float)(-1 - yStart), 0);
            return transform;
        }

        protected IGraphicsContext? Context;

        public virtual void Initialize(IGraphicsContext context)
        {
            if (IsInitialized)
            {
                return;
            }

            this.Context = context;
            GL.Enable(EnableCap.DepthTest);
            foreach (var coordinateRendererSeries in RenderSeriesCollection)
            {
                coordinateRendererSeries.Initialize(context);
            }

            //不管是否自动Y轴都要自动创建ssbo
            _yAxisCastSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _yAxisCastSSBO);
            GL.BufferData<int>(BufferTarget.ShaderStorageBuffer, _yAxisRaster.Length * sizeof(int), _yAxisRaster,
                BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, _yAxisCastSSBO);
            IsInitialized = true;
        }

        private Region2D _lastTargetRegion;

        /// <summary>
        /// 渲染的准备阶段，初始化、检查渲染快照变更和刷新缓冲区
        /// </summary>
        /// <returns></returns>
        public virtual bool PreviewRender()
        {
            if (_targetRegion2D.IsEmpty())
            {
                return false;
            }

            var renderEnable = false;
            if (!_lastTargetRegion.Equals(_targetRegion2D))
            {
                renderEnable = true;
                _lastTargetRegion = _targetRegion2D;
                RenderingRegion = _targetRegion2D;
            }

            renderEnable = _isHeightAdapting || renderEnable;
            //检查未初始化，当预渲染ok且渲染可用时表示渲染可用
            foreach (var renderSeries in RenderSeriesCollection)
            {
                if (!renderSeries.IsInitialized)
                {
                    renderSeries.Initialize(Context);
                }

                if (renderSeries.PreviewRender() && renderSeries.RenderEnable)
                {
                    renderEnable = true;
                }
            }

            //对比渲染快照
            var rendererSeries = RenderSeriesCollection
                .Where(series => series.RenderEnable)
                .ToArray();
            if (!_rendererSeriesSnapList.SequenceEqual(rendererSeries))
            {
                renderEnable = true;
            }

            _rendererSeriesSnapList = rendererSeries;
            return renderEnable;
        }

        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        public virtual void Render(GlRenderEventArgs args)
        {
            GL.ClearColor(_backgroundColor4);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            if (_rendererSeriesSnapList.All(series => !series.AnyReadyRenders()))
            {
                return;
            }

            #region transform

            if (_isHeightAdapting)
            {
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _yAxisCastSSBO);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero,
                    _emptySsboBuffer.Length * sizeof(int),
                    _emptySsboBuffer);
            }

            foreach (var seriesItem in _rendererSeriesSnapList)
            {
                seriesItem.ApplyDirective(new RenderDirective2D() { Transform = _tempTransform });
                seriesItem.Render(args);
            }

            //自适应高度检查，每次都执行
            if (_autoYAxisEnableValue)
            {
                _isHeightAdapting = true;
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, _yAxisCastSSBO);
                var ptr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly);
                Marshal.Copy(ptr, _yAxisRaster, 0, _yAxisRaster.Length);
                GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);
                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
                int i;
                for (i = YAxisCastSSBOLength - 1; i > 0; i--)
                {
                    if (_yAxisRaster[i] == 1)
                    {
                        break;
                    }
                }

                var renderingRegion = RenderingRegion;
                Region2D newRegion;
                var regionYRange = renderingRegion.YRange;
                if (i == 0) //表示界面内已没有任何元素，不需要适配
                {
                    if (regionYRange.Equals(this._defaultAxisYRangeValue))
                    {
                        _isHeightAdapting = false;
                        return;
                    }

                    newRegion = renderingRegion.ChangeYRange(this._defaultAxisYRangeValue);
                }
                else
                {
                    /* 1. 当最高点依然在SSBO的299位置时，应用变换然后在下次渲染再次检查，直到最高点不在299
                       2. 每次都检查，当差额小于特定百分比时停止变换。*/
                    var currentHeight = regionYRange.Range;
                    var ratio = i / (double)NdcYAxisSpacialSplitCount;
                    var theoreticalHeight = ratio * currentHeight / AutoAxisYContentRatio;
                    var precisionValue = currentHeight * AutoAxisYContentRatioAccuracy;
                    var abs = Math.Abs(currentHeight - theoreticalHeight);
                    // Debug.WriteLine($"{abs:F2},{precisionValue:F2},{currentHeight:F2},{theoreticalHeight:F2}");
                    if (abs < precisionValue)
                    {
                        _isHeightAdapting = false;
                        return;
                    }

                    newRegion = renderingRegion.ChangeYRange(new ScrollRange(regionYRange.Start,
                        regionYRange.Start + theoreticalHeight));
                }

                RenderingRegion = newRegion;
            }

            #endregion
        }

        public virtual void Resize(PixelSize size)
        {
            GL.Viewport(0, 0, size.Width, size.Height);
        }

        public virtual void Uninitialize()
        {
            foreach (var coordinateRendererSeries in RenderSeriesCollection)
            {
                coordinateRendererSeries.Uninitialize();
            }
        }

        public bool IsInitialized { get; protected set; }
    }
}