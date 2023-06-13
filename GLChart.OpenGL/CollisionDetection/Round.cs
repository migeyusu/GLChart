using System;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.CollisionDetection
{
    /// <summary>
    /// 用于碰撞检测的圆形
    /// </summary>
    public class Round : ICollisionGeometry2D
    {
        public Point2D Center { get; set; }

        public Boundary2D OrthogonalBoundary { get; set; }

        public float Radius { get; }

        public Round(Point2D point, float radius)
        {
            Center = point;
            Radius = radius;
            OrthogonalBoundary = new Boundary2D(point.X - radius, point.X + radius, point.Y - radius, point.Y + radius);
        }

        public bool Contain(Point2D point)
        {
            return Math.Sqrt((Math.Pow(Math.Abs(this.Center.X - point.X), 2) +
                              Math.Pow(Math.Abs(this.Center.Y - point.Y), 2))) < Radius;
        }
    }
}