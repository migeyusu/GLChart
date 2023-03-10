using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using RLP.Chart.Interface;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.CollisionDetection;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL
{
    public static class GeometryExtension
    {
        /// <summary>
        /// 2d translation
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Matrix3 CreateTranslation(this Matrix3 identity, float x, float y)
        {
            identity.Row2.X = x;
            identity.Row2.Y = y;
            return identity;
        }

        public static Boundary2D ToBoundary(this Region2D region)
        {
            return new Boundary2D((float)region.Left, (float)region.Right, (float)region.Bottom, (float)region.Top);
        }


        /// <summary>
        /// 得到距离的平方
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float GetDistanceSquare(this ICollisionGeometry2D geometry, Point2D point)
        {
            var centerPoint = geometry.Center;
            return centerPoint.GetDistanceSquare(point);
        }

        public static Node? NearestNodeData(this ICollisionGeometry2D geometry, IEnumerable<ICollisionCell> cells,
            out float squareDistance)
        {
#if priority
            //优先搜索所在的cell？
            if (!TryGetIndex(point, out var rowIndex, out var columnIndex))
            {
                return false;
            }
            var pointLocatedCell = Cells[rowIndex, columnIndex];
            var cells = roundGeometry.OrthogonalBoundary.IsSubSetOf(pointLocatedCell.Boundary) //优先搜索的必要性？
                ? pointLocatedCell.AccuracyGradeSearch(point, distance).ToArray()
                : GetGeometryCells(roundGeometry).SelectMany(cell => cell.DataCollection).ToArray();
#endif

            return NearestNodeData(geometry, cells.SelectMany((cell => cell.DataCollection)), out squareDistance);
        }

        public static Node? NearestNodeData(this ICollisionGeometry2D geometry, IEnumerable<Node> nodeDataCollection,
            out float squareDistance)
        {
            Node? node = null;
            squareDistance = float.MaxValue;
            var centerPoint = geometry.Center;
            foreach (var data in nodeDataCollection)
            {
                var dataPoint = data.Point;
                if (geometry.Contain(dataPoint))
                {
                    var sqrt = dataPoint.GetDistanceSquare(centerPoint);
                    if (sqrt <= squareDistance)
                    {
                        squareDistance = sqrt;
                        node = data;
                    }
                }
            }

            return node;
        }

        public static Point2D ToStructurePoint(this IPoint2D point)
        {
            return new Point2D(point.X, point.Y);
        }
    }
}