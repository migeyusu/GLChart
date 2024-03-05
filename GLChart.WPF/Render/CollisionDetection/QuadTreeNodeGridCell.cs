using System.Collections.Generic;
using System.Linq;

namespace GLChart.WPF.Render.CollisionDetection
{
    public class QuadTreeNodeGridCell : ICollisionCell
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
        public IEnumerable<Point2DNode> DataCollection => PointsTree.TraverseNotEmpty().Select(node => node.Data.Value);

        public void Insert(Point2DNode node)
        {
            PointsTree.Insert(node);
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public void Remove(Point2DNode node)
        {
            throw new System.NotImplementedException();
        }
    }
}