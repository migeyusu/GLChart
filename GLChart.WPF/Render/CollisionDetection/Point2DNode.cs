using GLChart.WPF.Base;

namespace GLChart.WPF.Render.CollisionDetection
{
    /// <summary>
    ///  Point2D的碰撞检测适配器
    /// </summary>
    public readonly struct Point2DNode
    {
        public IPoint2D Data { get; }

        /// <summary>
        /// 使用副本而不是直接使用<see cref="IPoint2D"/>是为了防止装箱
        /// </summary>
        public Point2D Point { get; }

        public Point2DNode(IPoint2D point)
        {
            Point = new Point2D(point.X, point.Y);
            this.Data = point;
        }

        public Point2DNode(Point2D point)
        {
            this.Point = point;
            this.Data = null;
        }

        public bool Equals(Point2DNode other)
        {
            return Point.Equals(other.Point);
        }

        public override bool Equals(object obj)
        {
            return obj is Point2DNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode();
        }
    }
}