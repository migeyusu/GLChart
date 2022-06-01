using System.Collections.Generic;
using System.Linq;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.CollisionDetection
{
    public class QuadTreeNodeGridCell : ICell
    {
        public QuadTreeNode PointsTree { get; }

        public QuadTreeNodeGridCell(Boundary2D boundary)
        {
            PointsTree = new QuadTreeNode(boundary);
        }

        public int Id { get; set; }

        public Boundary2D Boundary => PointsTree.Boundary;

        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public IEnumerable<Node> DataCollection => PointsTree.TraverseNotEmpty().Select(node => node.Data.Value);

        public void Insert(Node node)
        {
            PointsTree.Insert(node);
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public void Remove(Node node)
        {
            throw new System.NotImplementedException();
        }
    }
}