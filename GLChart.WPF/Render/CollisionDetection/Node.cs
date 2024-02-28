using GLChart.WPF.Base;

namespace GLChart.WPF.Render.CollisionDetection
{
    /// <summary>
    /// <see cref="IPoint2D"/>的装箱
    /// </summary>
    public readonly struct Node
    {
        public bool Equals(Node other)
        {
            return Point.Equals(other.Point);
        }

        public override bool Equals(object obj)
        {
            return obj is Node other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode();
        }

        public IPoint2D Data { get; }
        
        /// <summary>
        /// 使用副本而不是直接使用<see cref="IPoint2D"/>是为了防止装箱
        /// </summary>
        public Point2D Point { get; }

        public Node(IPoint2D point)
        {
            Point = new Point2D(point.X, point.Y);
            this.Data = point;
        }

        public Node(Point2D point)
        {
            this.Point = point;
            this.Data = null;
        }
    }
}