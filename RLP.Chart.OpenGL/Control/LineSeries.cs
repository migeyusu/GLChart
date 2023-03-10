using System.Collections.Generic;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.CollisionDetection;

namespace RLP.Chart.OpenGL.Control
{
    /// <summary>
    /// 增加了碰撞检测，允许鼠标和点的交互
    /// </summary>
    public class LineSeries : LineSeriesBase
    {
        public LineSeries(IPoint2DCollisionLayer collisionGridLayer)
        {
            CollisionGridLayer = collisionGridLayer;
        }

        public IPoint2DCollisionLayer CollisionGridLayer { get; }

        public override void Add(IPoint2D point)
        {
            base.Add(point);
            CollisionGridLayer.Add(point);
        }

        public override void AddRange(IList<IPoint2D> points)
        {
            base.AddRange(points);
            CollisionGridLayer.AddRange(points);
        }

        public override void ResetWith(IPoint2D geometry)
        {
            base.ResetWith(geometry);
            CollisionGridLayer.ResetWith(geometry);
        }

        public override void ResetWith(IList<IPoint2D> geometries)
        {
            base.ResetWith(geometries);
            CollisionGridLayer.ResetWith(geometries);
        }

        public override void Clear()
        {
            base.Clear();
            this.CollisionGridLayer.Clear();
        }
    }
}