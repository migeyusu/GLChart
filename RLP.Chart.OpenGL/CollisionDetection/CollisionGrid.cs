using System.Collections.Generic;
using RLP.Chart.OpenGL.Renderer;


namespace RLP.Chart.OpenGL.CollisionDetection
{
    public class CollisionGrid
    {
        private readonly List<IPoint2DCollisionLayer> _layers = new List<IPoint2DCollisionLayer>(10);

        public CollisionGrid()
        {
        }

        public void AddLayer(IPoint2DCollisionLayer layer)
        {
            _layers.Add(layer);
        }

        public void Remove(IPoint2DCollisionLayer layer)
        {
            _layers.Remove(layer);
        }

        public void Clear()
        {
            _layers.Clear();
        }

        public bool TrySearch(ICollisionGeometry2D geometry, out Node node, out IPoint2DCollisionLayer fromLayer)
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