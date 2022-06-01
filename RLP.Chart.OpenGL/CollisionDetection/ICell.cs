using System.Collections.Generic;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.CollisionDetection
{
    public interface ICell
    {
        int RowIndex { get; set; }

        int ColumnIndex { get; set; }

        IEnumerable<Node> DataCollection { get; }

        void Insert(Node node);

        void Remove(Node node);

        void Clear();
    }

}