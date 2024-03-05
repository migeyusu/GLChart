using System.Collections.Generic;

namespace GLChart.WPF.Render.CollisionDetection
{
    /// <summary>
    /// 碰撞网格
    /// </summary>
    public interface ICollisionCell
    {
        int RowIndex { get; set; }

        int ColumnIndex { get; set; }

        IEnumerable<Point2DNode> DataCollection { get; }

        void Insert(Point2DNode node);

        void Remove(Point2DNode node);

        void Clear();
    }
}