using System.Collections.Generic;
using RLP.Chart.OpenGL.Renderer;


namespace RLP.Chart.OpenGL.CollisionDetection
{
    public class CollisionGridPoint2D
    {
        private readonly List<ICollisionPoint2D> _layers = new List<ICollisionPoint2D>(10);

        public CollisionGridPoint2D()
        {
        }

        public void AddLayer(ICollisionPoint2D layer)
        {
            _layers.Add(layer);
        }

        public void Remove(ICollisionPoint2D layer)
        {
            _layers.Remove(layer);
        }

        public void Clear()
        {
            _layers.Clear();
        }

        public bool TrySearch(ICollisionGeometry2D geometry, out Node node, out ICollisionPoint2D fromLayer)
        {
            foreach (var layer in _layers)
            {
                if (layer.TrySearch(geometry, out node))
                {
                    fromLayer = layer;
                    return true;
                }
            }

            fromLayer = null;
            node = default;
            return !node.Equals(default);
        }
    }
}