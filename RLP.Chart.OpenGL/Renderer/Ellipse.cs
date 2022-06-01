using System;

namespace RLP.Chart.OpenGL.Renderer
{
    public class Ellipse:Geometry2D
    {
       
        /// <summary>
        /// 原点
        /// </summary>
        public Point2D Origin { get; }

        public float A { get; }

        public float B { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="a">x轴截距</param>
        /// <param name="b">y轴截距</param>
        public Ellipse(Point2D point, float a, float b)
        {
            this.Center=point;
            Origin = point;
            A = a;
            B = b;
            OrthogonalBoundary=new CollisionDetection.Boundary2D(point.X - a,point.X+a,point.Y-b,point.Y+b);
        }

        /// <summary>
        /// point locate in or on this ellipse
        /// </summary>
        /// <param name="checkPoint"></param>
        /// <returns></returns>
        public override bool Contain(Point2D checkPoint)
        {
            return Math.Pow(Math.Abs(checkPoint.X - Origin.X), 2) / Math.Pow(A, 2)
                + Math.Pow(Math.Abs(checkPoint.Y - Origin.Y), 2) / Math.Pow(B, 2) <= 1;
        }
    }
}