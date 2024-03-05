using System.Collections.Generic;
using GLChart.WPF.Base;

namespace GLChart.WPF.Render.CollisionDetection
{
    public class CollisionGrid2D
    {
        private readonly List<ICollision2DLayer> _layers = new List<ICollision2DLayer>(10);

        public CollisionGrid2D()
        {
        }

        public void AddLayer(ICollision2DLayer layer)
        {
            _layers.Add(layer);
        }

        public void Remove(ICollision2DLayer layer)
        {
            _layers.Remove(layer);
        }

        public void Clear()
        {
            _layers.Clear();
        }

        public bool TrySearch(MouseCollisionEllipse geometry, out IGeometry2D? node, out ICollision2DLayer? fromLayer)
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
            node = null;
            return false;
        }
    }
}