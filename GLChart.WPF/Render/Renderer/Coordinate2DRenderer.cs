using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class Coordinate2DRenderer : IRenderer
    {
        /// <summary>
        /// 事件在非UI线程上被调用,actual region变化时触发。
        /// </summary>
        public event Action<ScrollRange> ActualYRangeChanged;

        /// <summary>
        /// 点采样函数，渲染大量点位时允许只渲染部分点，todo:未实现
        /// </summary>
        public Func<int, ScrollRange> SamplingFunction { get; set; }

        public bool AutoYAxisEnable { get; set; }

        /// <summary>
        /// 自适应Y轴的默认区间，当界面内没有元素时显示该区间
        /// </summary>
        public ScrollRange DefaultAxisYRange { get; set; } = new ScrollRange(0, 100);

        private ScrollRange _actualYRange;

        /// <summary>
        /// 当前渲染器的实际坐标，如果自动高度，<see cref="ActualYRange"/>和<see cref="TargetYRange"/>会不同
        /// </summary>
        public ScrollRange ActualYRange
        {
            get => _actualYRange;
            protected set
            {
                if (_actualYRange.Equals(value))
                {
                    return;
                }

                _actualYRange = value;
                OnActualYRangeChanged(value);
            }
        }

        public ScrollRange TargetXRange
        {
            get => _targetXRange;

            set
            {
                if (value.Equals(_targetXRange))
                {
                    return;
                }

                _targetXRange = value;
                _targetRegion2D = _targetRegion2D.ChangeXRange(value);
            }
        }

        public ScrollRange TargetYRange
        {
            get => _targetYRange;

            set
            {
                if (value.Equals(_targetYRange))
                {
                    return;
                }

                _targetYRange = value;
                _targetRegion2D = _targetRegion2D.ChangeYRange(value);
            }
        }

        private Region2D _targetRegion2D;

        /// <summary>
        /// 当前渲染器的设定坐标
        /// </summary>
        public Region2D TargetRegion
        {
            get { return _targetRegion2D; }
        }

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
                this.ActualYRange = value.YRange;
                this._tempTransform = GetTransform(value);
            }
        }

        /// <summary>
        /// 渲染快照
        /// </summary>
        private IList<BaseRenderer> _rendererSeriesSnapList = new List<BaseRenderer>();

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

        public Color4 BackgroundColor { get; set; } = Color4.White;

        public IReadOnlyCollection<BaseRenderer> Series =>
            new ReadOnlyCollection<BaseRenderer>(RenderSeriesCollection);

        protected readonly IList<BaseRenderer> RenderSeriesCollection;

        public Coordinate2DRenderer(IList<BaseRenderer> renderSeriesCollection)
        {
            RenderSeriesCollection = renderSeriesCollection;
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

        protected IGraphicsContext Context;

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
        private ScrollRange _targetXRange;
        private ScrollRange _targetYRange;

        /// <summary>
        /// 渲染的准备阶段，初始化、检查渲染快照变更和刷新缓冲区
        /// </summary>
        /// <returns></returns>
        public virtual bool PreviewRender()
        {
            if (TargetRegion.IsEmpty())
            {
                return false;
            }

            var renderEnable = false;
            if (!_lastTargetRegion.Equals(TargetRegion))
            {
                renderEnable = true;
                _lastTargetRegion = TargetRegion;
                RenderingRegion = TargetRegion;
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
        /// 基本思路:当Y轴非自适应时，<see cref="TargetRegion"/>都变为<see cref="ActualRegion"/>，并更新transform；
        /// <para>当Y轴为自适应时，<see cref="TargetRegion"/>的设置将标记更新，由render过程维护transform</para>
        /// </summary>
        /// <param name="args"></param>
        public virtual void Render(GlRenderEventArgs args)
        {
            GL.ClearColor(BackgroundColor);
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

            //自适应高度检查
            if (AutoYAxisEnable)
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
                    if (regionYRange.Equals(this.DefaultAxisYRange))
                    {
                        _isHeightAdapting = false;
                        return;
                    }

                    newRegion = renderingRegion.ChangeYRange(this.DefaultAxisYRange);
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

        protected virtual void OnActualYRangeChanged(ScrollRange obj)
        {
            ActualYRangeChanged?.Invoke(obj);
        }
    }
}