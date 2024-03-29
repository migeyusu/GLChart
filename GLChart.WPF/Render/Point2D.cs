﻿using System;
using GLChart.WPF.Base;
using GLChart.WPF.Render.CollisionDetection;

namespace GLChart.WPF.Render
{
    /// <summary>
    /// 2d 点
    /// </summary>
    public readonly struct Point2D : IPoint2D
    {
        public float X { get; }

        public float Y { get; }

        public Point2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// 座位于，位于区间
        /// </summary>
        /// <param name="boundary"></param>
        /// <returns></returns>
        public bool IsLocatedIn(Boundary2D boundary)
        {
            //由于浮点数的原因，比较可能出现漂移，对于开放的boundary需要处理
            return boundary.XAxleSegment.Contains(X) && boundary.YAxleSegment.Contains(Y);
        }

        public Boundary2D CreateWrapperBoundary(Boundary2D boundary, float rowOverHead, float columnOverHead)
        {
            var xLow = boundary.XLow;
            var xHigh = boundary.XHigh;
            var yLow = boundary.YLow;
            var yHigh = boundary.YHigh;
            if (xLow > X)
            {
                xLow = X;
            }
            else if (xHigh <= X)
            {
                xHigh = X + columnOverHead;
            }

            if (yLow > Y)
            {
                yLow = Y;
            }
            else if (yHigh <= Y)
            {
                yHigh = Y + rowOverHead;
            }

            return new Boundary2D(xLow, xHigh, yLow, yHigh);
        }

        /// <summary>
        /// 得到距离的平方
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public float GetDistanceSquare(Point2D point)
        {
            return (float)(Math.Pow(Math.Abs(point.X - point.X), 2) +
                           Math.Pow(Math.Abs(point.Y - point.Y), 2));
        }

        private static readonly Random Random = new Random();

        public static Point2D RandomFrom(Boundary2D boundary)
        {
            var nextDouble = Random.NextDouble();
            var x = nextDouble * boundary.XSpan + boundary.XLow;
            var y = Random.NextDouble() * boundary.YSpan + boundary.YLow;
            return new Point2D((float)x, (float)y);
        }

        public override string ToString()
        {
            return $"X:{X},Y{Y}";
        }

        public bool Equals(Point2D other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            return obj is Point2D other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }
    }
}