using System;
using System.Collections.Generic;
using System.Linq;
using GLChart.Core.Collection;
using GLChart.Core.Renderer;

namespace GLChart.Core.CollisionDetection
{
    public class LinkedListGridCell : ICollisionCell
    {
        private readonly SingleLinkedList<Node> _nodesLinkedList = new SingleLinkedList<Node>();

        public LinkedListGridCell()
        {
        }

        public int Id { get; set; }

        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public IEnumerable<Node> DataCollection => _nodesLinkedList;

        public void Insert(Node node)
        {
            _nodesLinkedList.Append(node);
        }

        public void Clear()
        {
            _nodesLinkedList.Clear();
        }

        public void Remove(Node node)
        {
            _nodesLinkedList.Remove(node);
        }

        public IEnumerable<Node> AccuracyGradeSearch(Point2D point, float accuracy)
        {
            var distanceSquare = Math.Pow(accuracy, 2);
            return _nodesLinkedList.Where(node => node.Point.GetDistanceSquare(point) <= distanceSquare);
        }
    }
}