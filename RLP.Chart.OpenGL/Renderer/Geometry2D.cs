using RLP.Chart.OpenGL.CollisionDetection;

namespace RLP.Chart.OpenGL.Renderer
{
    public abstract class Geometry2D
    {
        /// <summary>
        /// 中心
        /// </summary>
        public Point2D Center { get;protected set;}

        public Boundary2D OrthogonalBoundary { get; protected set;}

        public abstract bool Contain(Point2D point) ;

    }
}