using System;
using RLP.Chart.OpenGL.CollisionDetection;

namespace RLP.Chart.OpenGL.Renderer
{
    public class Round : Geometry2D
    {
        public float Radius { get; }

        public Round(Point2D point, float radius)
        {
            Center = point;
            Radius = radius;
            OrthogonalBoundary = new Boundary2D(point.X - radius, point.X + radius, point.Y - radius, point.Y + radius);
        }

        public override bool Contain(Point2D point)
        {
            return Math.Sqrt((Math.Pow(Math.Abs(this.Center.X - point.X), 2) +
                              Math.Pow(Math.Abs(this.Center.Y - point.Y), 2))) < Radius;
        }
    }
}