using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Collection;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.CollisionDetection
{
    /// <summary>
    /// 使用稀疏网格的碰撞算法，todo：当前只实现于大于零的情况
    /// <para>使用x+y mod </para>
    /// <para>x^y mod</para>
    /// </summary>
    public class SpacialHashCollisionPoint2DLayer : ICollisionPoint2D
    {
        protected bool Equals(SpacialHashCollisionPoint2DLayer other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SpacialHashCollisionPoint2DLayer)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        private readonly double _cellSize;
        private readonly int _tableSize;

        private readonly Algorithm _algorithm;

        private readonly SingleLinkedList<LinkedListGridCell>[] _units;

        // public const double 

        /// <summary>
        /// table size 越大性能越好
        /// </summary>
        /// <param name="cellSize"></param>
        /// <param name="algorithm"></param>
        /// <param name="tableSize"></param>
        public SpacialHashCollisionPoint2DLayer(double cellSize, Algorithm algorithm, int tableSize = 10000)
        {
            this._cellSize = cellSize;
            this._tableSize = tableSize;
            this._algorithm = algorithm;
            this._units = new SingleLinkedList<LinkedListGridCell>[tableSize];
        }

        private ICollisionCell GetCell(int rowIndex, int columnIndex)
        {
            var tableUnitIndex = _algorithm.Get(rowIndex, columnIndex) % _tableSize;
            tableUnitIndex = Math.Abs(tableUnitIndex);
            var singleLinkedList = _units[tableUnitIndex];
            var singleLinkedNode =
                singleLinkedList?.Find((cell => cell.ColumnIndex == columnIndex && cell.RowIndex == rowIndex));
            return singleLinkedNode?.Data;
        }

        public void Insert(ICollisionGeometry2D geometry)
        {
            throw new NotImplementedException();
        }

        public void RemoveFromAxisX(double x)
        {
            var startColumnIndex = (int)Math.Floor(x / _cellSize);
            foreach (var gridCell in _units)
            {
                if (gridCell != null)
                {
                    gridCell.Remove(cell => cell.ColumnIndex < startColumnIndex);
                }
            }
        }

        public IEnumerable<ICollisionCell> GetCollisionCells(ICollisionGeometry2D geometry)
        {
            return GetCollisionCells(geometry.OrthogonalBoundary);
        }

        public IEnumerable<ICollisionCell> GetCollisionCells(Boundary2D boundary)
        {
            var startColumnIndex = (int)Math.Floor(boundary.XLow / _cellSize);
            var startRowIndex = (int)Math.Floor(boundary.YLow / _cellSize);
            var endColumnIndex = Math.Ceiling(boundary.XHigh / _cellSize);
            var endRowIndex = (int)Math.Ceiling(boundary.YHigh / _cellSize);
            for (int i = startRowIndex; i <= endRowIndex; i++)
            {
                for (int j = startColumnIndex; j < endColumnIndex; j++)
                {
                    var cell = GetCell(i, j);
                    if (cell != null)
                    {
                        yield return cell;
                    }
                }
            }
        }

        public Guid Id { get; } = Guid.NewGuid();

        public bool TrySearch(ICollisionGeometry2D geometry, out Node node)
        {
            var collisionCells = this.GetCollisionCells(geometry);
            var nearestNodeData = geometry.NearestNodeData(collisionCells, out _);
            if (nearestNodeData.HasValue)
            {
                node = nearestNodeData.Value;
                return true;
            }

            node = default;
            return false;
        }

        public void Add(IPoint2D point)
        {
            var columnIndex = (int)Math.Floor(point.X / _cellSize);
            var rowIndex = (int)Math.Floor(point.Y / _cellSize);
            var hash = _algorithm.Get(rowIndex, columnIndex);
            var tableUnitIndex = hash % _tableSize;
            var singleLinkedList = _units[tableUnitIndex];
            if (singleLinkedList == null)
            {
                singleLinkedList = new SingleLinkedList<LinkedListGridCell>();
                _units[tableUnitIndex] = singleLinkedList;
            }

            LinkedListGridCell containerCell;
            var singleLinkedNode =
                singleLinkedList.Find(cell => cell.ColumnIndex == columnIndex && cell.RowIndex == rowIndex);
            if (singleLinkedNode == null)
            {
                containerCell = new LinkedListGridCell() { ColumnIndex = columnIndex, RowIndex = rowIndex };
                singleLinkedList.Append(containerCell);
            }
            else
            {
                containerCell = singleLinkedNode.Data;
            }

            containerCell.Insert(new Node(point));
        }

        public void AddRange(IList<IPoint2D> nodes)
        {
            foreach (var point in nodes)
            {
                Add(point);
            }
        }

        public void ResetWith(IPoint2D node)
        {
            this.Clear();
            this.Add(node);
        }

        public void ResetWith(IList<IPoint2D> nodes)
        {
            this.Clear();
            this.AddRange(nodes);
        }

        public void Clear()
        {
            for (var index = 0; index < _units.Length; index++)
            {
                var cell = _units[index];
                cell?.Clear();
            }
        }


        public abstract class Algorithm
        {
            public static Algorithm Plus => new PlusAlgorithm();

            public static Algorithm XOR => new XorAlgorithm();

            public static Algorithm XMapping => new XMappingAlgorithm();

            public static Algorithm Block => new BlockAlgorithm();

            public static Algorithm ChessBoard => new ChessBoardAlgorithm();


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public abstract int Get(int rowIndex, int columnIndex);
        }

        public class PlusAlgorithm : Algorithm
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int Get(int rowIndex, int columnIndex)
            {
                return rowIndex + columnIndex;
            }
        }

        public class XorAlgorithm : Algorithm
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int Get(int rowIndex, int columnIndex)
            {
                var hash = rowIndex * 73856093;
                hash = (hash ^ columnIndex) * 19349663;
                return hash;
            }
        }

        /// <summary>
        /// 将x轴映射到table size，当移除特定x轴的边界点位时只需要检查某个hash bucket
        /// </summary>
        public class XMappingAlgorithm : Algorithm
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int Get(int rowIndex, int columnIndex)
            {
                return columnIndex;
            }
        }

        /// <summary>
        /// 将cells约束在一个矩形范围内
        /// </summary>
        public class BlockAlgorithm : Algorithm
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int Get(int rowIndex, int columnIndex)
            {
                return Math.Max(rowIndex, columnIndex);
            }
        }

        /// <summary>
        /// 棋盘网格
        /// </summary>
        public class ChessBoardAlgorithm : Algorithm
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int Get(int rowIndex, int columnIndex)
            {
                return rowIndex - columnIndex;
            }
        }
    }
}