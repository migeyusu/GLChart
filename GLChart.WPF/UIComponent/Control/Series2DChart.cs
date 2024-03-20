using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using GLChart.WPF.Render;
using GLChart.WPF.Render.CollisionDetection;
using GLChart.WPF.Render.Renderer;

namespace GLChart.WPF.UIComponent.Control
{
    public class Series2DChart : Chart2DCore, ISeries2DChart
    {
        static Series2DChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Series2DChart),
                new FrameworkPropertyMetadata(typeof(Series2DChart)));
        }

        private readonly RingLine2DSeriesRenderer _ringLine2DSeriesRenderer 
            = new RingLine2DSeriesRenderer(new Shader("Render/Shaders/LineShader/shader.vert",
                "Render/Shaders/LineShader/shader.frag"));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.CoordinateRenderer!.SeriesRenderers.Add(_ringLine2DSeriesRenderer);
        }

        #region collision

        public CollisionEnum CollisionEnum { get; set; } = CollisionEnum.SpacialHash;

        /// <summary>
        /// “碰撞种子” ,影响碰撞检测的性能
        /// </summary>
        public Boundary2D CollisionSeed { get; set; } = new Boundary2D(0, 100, 0, 100);

        /// <summary>
        /// 初始化碰撞检测的边界，减少碰撞网格的分配开销
        /// </summary>
        public Boundary2D InitialCollisionGridBoundary { get; set; } = new Boundary2D(0, 100, 0, 100);

        #endregion
        
        #region collection

        public IReadOnlyList<ISeries2D> SeriesItems =>
            new ReadOnlyCollection<ISeries2D>(Series2Ds);

        public T NewSeries<T>() where T : ISeries2D
        {
            var collisionSeed = this.CollisionSeed;
            ICollisionPoint2D collisionPoint2D;
            switch (CollisionEnum)
            {
                case CollisionEnum.SpacialHash:
                    collisionPoint2D = new SpacialHashCollisionPoint2DLayer(collisionSeed.XSpan,
                        SpacialHashCollisionPoint2DLayer.Algorithm.XMapping,
                        (int)InitialCollisionGridBoundary.XSpan);
                    break;
                case CollisionEnum.UniformGrid:
                    collisionPoint2D = new CollisionGridPoint2DLayer(InitialCollisionGridBoundary,
                        collisionSeed.XSpan, collisionSeed.YSpan, new LinkedListGridCellFactory());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var type = typeof(T);
            if (type == typeof(RingLine2DRenderer))
            {
                var lineRenderer = new RingLine2DRenderer(collisionPoint2D);
                this.CollisionGrid.AddLayer(lineRenderer.CollisionLayer);
                _ringLine2DSeriesRenderer.Add(lineRenderer);
                this.Series2Ds.Add(lineRenderer);
                return (T)(ISeries2D)lineRenderer;
            }

            throw new NotSupportedException();
        }

        public void Remove(ISeries2D series)
        {
            if (series is RingLine2DRenderer lineRenderer)
            {
                this.Series2Ds.Remove(lineRenderer);
                _ringLine2DSeriesRenderer.Remove(lineRenderer);
                this.CollisionGrid.Remove(lineRenderer.CollisionLayer);
            }
        }

        public void Clear()
        {
            this.Series2Ds.Clear();
            this._ringLine2DSeriesRenderer.Clear();
            this.CollisionGrid.Clear();
        }

        #endregion
    }
}