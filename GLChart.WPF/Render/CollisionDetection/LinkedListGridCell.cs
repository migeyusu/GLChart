using System;
using System.Collections.Generic;
using System.Linq;
using GLChart.WPF.Render.Allocation;

namespace GLChart.WPF.Render.CollisionDetection
{
    public class LinkedListGridCell : ICollisionCell
    {
        private readonly SingleLinkedList<Point2DNode> _nodesLinkedList = new SingleLinkedList<Point2DNode>();

        public LinkedListGridCell()
        {
        }

        public int Id { get; set; }

        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public IEnumerable<Point2DNode> DataCollection => _nodesLinkedList;

        public void Insert(Point2DNode node)
        {
            _nodesLinkedList.Append(node);
        }

        public void Clear()
        {
            _nodesLinkedList.Clear();
        }

        public void Remove(Point2DNode node)
        {
            _nodesLinkedList.Remove(node);
        }

        public IEnumerable<Point2DNode> AccuracyGradeSearch(Point2D point, float accuracy)
        {
            var distanceSquare = Math.Pow(accuracy, 2);
            return _nodesLinkedList.Where(node => node.Point.GetDistanceSquare(point) <= distanceSquare);
        }
    }
}