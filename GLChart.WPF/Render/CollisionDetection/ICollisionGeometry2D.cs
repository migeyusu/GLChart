using GLChart.WPF.Base;

namespace GLChart.WPF.Render.CollisionDetection
{
    public interface ICollisionGeometry2D : IGeometry
    {
        /// <summary>
        /// 中心
        /// </summary>
        Point2D Center { get; set; }

        Boundary2D OrthogonalBoundary { get; set; }

        bool Contain(Point2D point);
    }
}