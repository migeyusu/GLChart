﻿using GLChart.WPF.Base;
using System;

namespace GLChart.WPF.Render.CollisionDetection
{
    /// <summary>
    /// 代表鼠标的碰撞检测区域  
    /// </summary>
    public class MouseCollisionEllipse : IGeometry2D
    {
        public Point2D Center { get; set; }

        public Boundary2D OrthogonalBoundary { get; set; }

        /// <summary>
        /// 原点
        /// </summary>
        public Point2D Origin { get; }

        public float A { get; }

        public float B { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="a">x轴截距</param>
        /// <param name="b">y轴截距</param>
        public MouseCollisionEllipse(Point2D point, float a, float b)
        {
            this.Center = point;
            Origin = point;
            A = a;
            B = b;
            OrthogonalBoundary = new Boundary2D(point.X - a, point.X + a, point.Y - b, point.Y + b);
        }

        /// <summary>
        /// point locate in or on this ellipse
        /// </summary>
        /// <param name="checkPoint"></param>
        /// <returns></returns>
        public bool Contain(Point2D checkPoint)
        {
            return Math.Pow(Math.Abs(checkPoint.X - Origin.X), 2) / Math.Pow(A, 2)
                + Math.Pow(Math.Abs(checkPoint.Y - Origin.Y), 2) / Math.Pow(B, 2) <= 1;
        }
    }
}