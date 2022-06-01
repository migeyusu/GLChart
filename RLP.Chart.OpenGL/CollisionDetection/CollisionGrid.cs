using System.Collections.Generic;
using RLP.Chart.OpenGL.Renderer;


namespace RLP.Chart.OpenGL.CollisionDetection
{
    public class CollisionGrid
    {
        private readonly List<ICollisionLayer> _layers = new List<ICollisionLayer>(10);

        public CollisionGrid()
        {
        }

        public void AddLayer(ICollisionLayer layer)
        {
            _layers.Add(layer);
        }

        public void Remove(ICollisionLayer layer)
        {
            _layers.Remove(layer);
        }

        public void Clear()
        {
            _layers.Clear();
        }

        public bool TrySearch(Geometry2D geometry, out Node node, out ICollisionLayer fromLayer)
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