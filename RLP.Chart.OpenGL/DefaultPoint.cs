using RLP.Chart.Interface.Abstraction;

namespace RLP.Chart.OpenGL
{
    public class DefaultPoint : IPoint2D
    {
        public float X
        {
            get => _point.X;
        }

        public float Y
        {
            get => _point.Y;
        }

        private Renderer.Point2D _point;

        public DefaultPoint(Renderer.Point2D point)
        {
            this._point = point;
        }
    }
}