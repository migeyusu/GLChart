using System;
using System.Collections.Generic;
using System.Linq;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.CollisionDetection
{
    /// <summary>
    /// 用于静态点位碰撞检测,grid的坐标系同opengl相同，既xy指向右和上为正数
    /// </summary>
    public class CollisionGridLayer : ICollisionLayer
    {
        public Guid Id { get; } = Guid.NewGuid();

        private readonly CellFactory _cellFactory;
        
        public CollisionGridLayer(Boundary2D boundary, float xStep, float yStep, CellFactory cellFactory)
        {
            var xCount = (int) Math.Round(boundary.XSpan / xStep);
            var yCount = (int) Math.Round(boundary.YSpan / yStep);
            this.Cells = CreateCells(boundary, xCount, yCount, cellFactory);
            this.Boundary = boundary;
            this.ColumnStep = xStep;
            this.RowStep = yStep;
            this._cellFactory = cellFactory;
        }

        public CollisionGridLayer(Boundary2D boundary, CellFactory cellFactory, int split = 10)
        {
            var xStep = (boundary.XSpan) / split;
            var yStep = (boundary.YSpan) / split;
            this.Cells = CreateCells(boundary, split, split, cellFactory);
            this.Boundary = boundary;
            this._cellFactory = cellFactory;
            this.ColumnStep = xStep;
            this.RowStep = yStep;
        }

        public Boundary2D Boundary { get; private set; }

        /// <summary>
        /// 使用二维数组表示grid 行,列形式
        /// </summary>
        public ICell[,] Cells { get; private set; }

        public float ColumnStep { get; private set; }

        public float RowStep { get; private set; }

        /// <summary>
        /// 顺序：从左到右
        /// </summary>
        public int ColumnsCount => Cells.GetLength(1);

        /// <summary>
        /// 顺序：从上到下
        /// </summary>
        public int RowsCount => Cells.GetLength(0);

        /// <summary>
        /// 修剪grid空间
        /// </summary>
        public void Clip(Boundary2D clip)
        {
            //添加空的cell
            if (!clip.IsSubSetOf(this.Boundary))
            {
                throw new ArgumentException("Can' clip boundary larger than grid");
            }

            var columnsCount = this.ColumnsCount;
            var newXStart = this.Boundary.XLow;
            var xLowClipCount = (int) Math.Floor((clip.XLow - this.Boundary.XLow) / ColumnStep);
            if (xLowClipCount > 0)
            {
                columnsCount -= xLowClipCount;
                newXStart = this.Boundary.XLow + xLowClipCount * this.ColumnStep;
            }

            var newXEnd = this.Boundary.XHigh;
            var xHighClipCount = (int) Math.Floor((this.Boundary.XHigh - clip.XHigh) / ColumnStep);
            if (xHighClipCount > 0)
            {
                columnsCount -= xHighClipCount;
                newXEnd -= xHighClipCount * ColumnStep;
            }

            var newYStart = this.Boundary.YLow;
            var rowsCount = this.RowsCount;
            var yLowClipCount = (int) Math.Floor((clip.YLow - this.Boundary.YLow) / RowStep);
            if (yLowClipCount > 0)
            {
                rowsCount -= yLowClipCount;
                newYStart -= yLowClipCount * RowStep;
            }

            var newYEnd = this.Boundary.YHigh;
            var yHighClipCount = (int) Math.Floor((this.Boundary.YHigh - clip.YHigh) / RowStep);
            if (yHighClipCount > 0)
            {
                rowsCount -= yHighClipCount;
                newYEnd -= yHighClipCount * RowStep;
            }

            var newBoundary = new Boundary2D(newXStart, newXEnd, newYStart, newYEnd);
            var cells = CreateCells(newBoundary, columnsCount, rowsCount, this._cellFactory);
            var clipRowCount = cells.GetLength(0);
            var clipColumnCount = cells.GetLength(1);
            for (int i = 0; i < clipRowCount; i++)
            {
                for (int j = 0; j < clipColumnCount; j++)
                {
                    cells[i, j] = this.Cells[i + yHighClipCount, j + xLowClipCount];
                }
            }

            this.Boundary = newBoundary;
            this.Cells = cells;
        }

        /// <summary>
        /// 分配grid空间
        /// </summary>
        public void Magnify(Boundary2D allocation)
        {
            if (!this.Boundary.IsSubSetOf(allocation))
            {
                return;
            }

            int xCount = this.ColumnsCount;
            int xStartIndex = 0;
            float newXStart = this.Boundary.XLow;
            var xLowComplement = this.Boundary.XLow - allocation.XLow;
            if (!xLowComplement.Equals(0f))
            {
                var complementCount = (int) Math.Ceiling(xLowComplement / this.ColumnStep);
                xCount += complementCount;
                xStartIndex = complementCount;
                newXStart -= complementCount * this.ColumnStep;
            }

            float newXEnd = this.Boundary.XHigh;
            var xHighComplement = allocation.XHigh - this.Boundary.XHigh;
            if (!xHighComplement.Equals(0f))
            {
                var complementCount = (int) Math.Ceiling(xHighComplement / this.ColumnStep);
                xCount += complementCount;
                newXEnd += complementCount * this.ColumnStep;
            }

            int yCount = this.RowsCount;
            float newYStart = this.Boundary.YLow;
            var yLowComplement = this.Boundary.YLow - allocation.YLow;
            if (!yLowComplement.Equals(0f))
            {
                var complementCount = (int) Math.Ceiling(yLowComplement / this.RowStep);
                yCount += complementCount;
                newYStart -= complementCount * this.RowStep;
            }

            int yStartIndex = 0;
            float newYEnd = this.Boundary.YHigh;
            var yHighComplement = allocation.YHigh - this.Boundary.YHigh;
            if (!yHighComplement.Equals(0f))
            {
                var complementCount = (int) Math.Ceiling(yHighComplement / this.RowStep);
                yCount += complementCount;
                yStartIndex = complementCount;
                newYEnd += complementCount * this.RowStep;
            }

            var newBoundary = new Boundary2D(newXStart, newXEnd, newYStart, newYEnd);
            //拷贝
            var cells = CreateCells(newBoundary, xCount, yCount, this._cellFactory);
            for (int i = 0; i < this.RowsCount; i++) //行
            {
                for (int j = 0; j < this.ColumnsCount; j++) //列
                {
                    cells[i + yStartIndex, j + xStartIndex] = this.Cells[i, j];
                }
            }

            this.Cells = cells;
            this.Boundary = newBoundary;
        }

        private static ICell[,] CreateCells(Boundary2D boundary, int columnCount, int rowCount, CellFactory cellFactory)
        {
            var boundaries = boundary.Divide(columnCount, rowCount);
            var cells = new ICell[rowCount, columnCount];
            for (var i = 0; i < rowCount; i++) //行
            {
                for (var j = 0; j < columnCount; j++) //列
                {
                    var index = columnCount * i + j;
                    cells[i, j] = cellFactory.CreateCell(boundaries[index]);
                }
            }

            return cells;
        }

        public bool TryAddNode(Node node)
        {
            if (!node.Point.IsLocatedIn(this.Boundary))
            {
                return false;
            }

            if (!TryGetIndex(node.Point, out var rowIndex, out var columnIndex))
            {
                return false;
            }

            AddNodeInternal(node, rowIndex, columnIndex);
            return false;
        }

        /// <summary>
        /// node point must located in boundary
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        private void AddNodeInternal(Node node, int rowIndex, int columnIndex)
        {
            var cell = Cells[rowIndex, columnIndex];
            cell.Insert(node);
        }

        /// <summary>
        /// 得到边界内的所有单元格
        /// </summary>
        /// <param name="boundary"></param>
        /// <returns></returns>
        private IEnumerable<ICell> GetBoundaryCells(Boundary2D boundary)
        {
            if (!TryGetColumnIndex(boundary.XLow, out int leftColumnIndex))
            {
                leftColumnIndex = 0;
            }

            if (!TryGetColumnIndex(boundary.XHigh, out int rightColumnIndex))
            {
                rightColumnIndex = ColumnsCount - 1;
            }

            if (!TryGetRowIndex(boundary.YLow, out int lowRowIndex))
            {
                lowRowIndex = this.RowsCount - 1;
            }

            if (!TryGetRowIndex(boundary.YHigh, out int upRowIndex))
            {
                upRowIndex = 0;
            }

            for (int i = upRowIndex; i <= lowRowIndex; i++)
            {
                for (int j = leftColumnIndex; j <= rightColumnIndex; j++)
                {
                    yield return Cells[i, j];
                }
            }
        }

        /// <summary>
        /// 得到几何体内关联的所有cell
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ICell> GetGeometryCells(Geometry2D geometry)
        {
            return GetBoundaryCells(geometry.OrthogonalBoundary);
        }

        private bool TryGetRowIndex(float yValue, out int rowIndex)
        {
            if (this.Boundary.YAxleSegment.TryGetTicks(this.RowStep, yValue, out rowIndex))
            {
                rowIndex = this.RowsCount - 1 - rowIndex;
                return true;
            }

            return false;
        }

        public bool TryGetColumnIndex(float xValue, out int columnIndex)
        {
            return this.Boundary.XAxleSegment.TryGetTicks(this.ColumnStep, xValue, out columnIndex);
        }

        private bool TryGetIndex(Point2D point, out int rowIndex, out int columnIndex)
        {
            var tryGetColumnIndex = TryGetColumnIndex(point.X, out columnIndex);
            var tryGetRowIndex = TryGetRowIndex(point.Y, out rowIndex);
            return tryGetColumnIndex && tryGetRowIndex;
        }

        /// <summary>
        /// 自适应地添加节点
        /// </summary>
        /// <param name="point"></param>
        public void AddNode(IPoint2D point)
        {
            var node = new Node(point);
            var nodePoint = node.Point;
            if (!nodePoint.IsLocatedIn(this.Boundary))
            {
                var expandBoundary = nodePoint.CreateWrapperBoundary(this.Boundary, this.RowStep, this.ColumnStep);
                Magnify(expandBoundary);
            }

            if (!TryGetIndex(node.Point, out var rowIndex, out var columnIndex))
            {
                var expandBoundary = nodePoint.CreateWrapperBoundary(this.Boundary, this.RowStep, this.ColumnStep);
                Magnify(expandBoundary);
            }

            AddNodeInternal(node, rowIndex, columnIndex);
        }


        public void AddNodes(IEnumerable<IPoint2D> nodeDataCollection)
        {
            foreach (var nodeData in nodeDataCollection)
            {
                AddNode(nodeData);
            }
        }

        /// <summary>
        /// 删除单元格内元素
        /// </summary>
        public void ClearNodes()
        {
            foreach (var cell in Cells)
            {
                cell.Clear();
            }
        }

        /// <summary>
        /// 该方法可能耗时较长
        /// </summary>
        public void Remove(Node node)
        {
            if (TryGetIndex(node.Point, out var rowIndex, out var columnIndex))
            {
                Cells[rowIndex, columnIndex].Remove(node);
            }
        }

        /// <summary>
        /// 清空指定边界内的点
        /// </summary>
        /// <param name="boundary"></param>
        public void ClearBoundary(Boundary2D boundary)
        {
            if (boundary.IsCrossed(this.Boundary))
            {
                foreach (var boundaryCell in GetBoundaryCells(boundary))
                {
                    var array = boundaryCell.DataCollection.Where(node => node.Point.IsLocatedIn(boundary));
                    foreach (var node in array)
                    {
                        boundaryCell.Remove(node);
                    }
                }
            }
        }

        /// <summary>
        /// 重置单元格元素
        /// </summary>
        public void ResetWithNodes(IEnumerable<IPoint2D> nodes)
        {
            this.ClearNodes();
            this.AddNodes(nodes);
        }

        public void ResetWithNode(IPoint2D node)
        {
            this.ClearNodes();
            this.AddNode(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool TrySearch(Geometry2D geometry, out Node node)
        {
            node = default;
            var geometryOrthogonalBoundary = geometry.OrthogonalBoundary;
            if (!geometryOrthogonalBoundary.IsCrossed(this.Boundary))
            {
                return false;
            }

            var boundaryCells = GetBoundaryCells(geometryOrthogonalBoundary);
            var nearestNodeData = geometry.NearestNodeData(boundaryCells, out _);
            if (nearestNodeData.HasValue)
            {
                node = nearestNodeData.Value;
                return true;
            }

            return false;
        }

        protected bool Equals(CollisionGridLayer other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CollisionGridLayer) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}