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

        IEnumerable<Node> DataCollection { get; }

        void Insert(Node node);

        void Remove(Node node);

        void Clear();
    }
}