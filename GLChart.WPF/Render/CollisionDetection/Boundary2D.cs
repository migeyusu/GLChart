using System.Collections.Generic;

namespace GLChart.WPF.Render.CollisionDetection
{
    
    /// <summary>
    /// 二维的<see cref="AxisSegment"/> 默认下边界封闭，上边界开放
    /// </summary>
    public readonly struct Boundary2D
    {
        public AxisSegment XAxleSegment { get; }

        public AxisSegment YAxleSegment { get; }

        public float XLow => XAxleSegment.Start.Value;

        public float XHigh => XAxleSegment.End.Value;

        public float YLow => YAxleSegment.Start.Value;

        public float YHigh => YAxleSegment.End.Value;

        public float XSpan => XAxleSegment.Length;

        public float YSpan => YAxleSegment.Length;

        // public bool IsSquare => Math.Abs(XSpan - YSpan) < 0.0000001; //float.Epsilon;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xLow">x轴低位</param>
        /// <param name="xHigh">x轴高位</param>
        /// <param name="yLow">y轴低位</param>
        /// <param name="yHigh">y轴高位</param>
        public Boundary2D(float xLow, float xHigh, float yLow, float yHigh)
        {
            XAxleSegment = new AxisSegment(new AxisSegmentEndPoint(xLow, true),
                new AxisSegmentEndPoint(xHigh, false));
            YAxleSegment = new AxisSegment(new AxisSegmentEndPoint(yLow, true),
                new AxisSegmentEndPoint(yHigh, false));
        }

        /// <summary>
        /// 是<paramref name="boundary"/>的子集
        /// </summary>
        /// <param name="boundary"></param>
        /// <returns></returns>
        public bool IsSubSetOf(Boundary2D boundary)
        {
            return boundary.XAxleSegment.Contains(this.XAxleSegment)
                   && boundary.YAxleSegment.Contains(this.YAxleSegment);
        }

        /// <summary>
        /// 从左到右，从上到下
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public IList<Boundary2D> Divide(int region)
        {
            var xStep = (XHigh - XLow) / region;
            var yStep = (YHigh - YLow) / region;
            var boundaries = new Boundary2D[region * region];
            float rowHigh, rowLow, columnHigh, columnLow;
            for (int i = 0; i < region; i++) //row
            {
                rowHigh = YHigh - i * yStep; //不要优化
                rowLow = YHigh - (i + 1) * yStep;
                for (int j = 0; j < region; j++) //column
                {
                    columnLow = XLow + xStep * j;
                    columnHigh = XLow + xStep * (j + 1);
                    boundaries[region * i + j] = new Boundary2D(columnLow, columnHigh, rowLow, rowHigh);
                }
            }

            return boundaries;
        }

        public IList<Boundary2D> Divide(int columnCount, int rowCount)
        {
            var xStep = (XHigh - XLow) / columnCount;
            var yStep = (YHigh - YLow) / rowCount;
            var boundaries = new Boundary2D[columnCount * rowCount];
            float rowHigh, rowLow, columnHigh, columnLow;
            for (int i = 0; i < rowCount; i++) //row
            {
                rowHigh = YHigh - i * yStep; //不要优化
                rowLow = YHigh - (i + 1) * yStep;
                for (int j = 0; j < columnCount; j++) //column
                {
                    columnLow = XLow + xStep * j;
                    columnHigh = XLow + xStep * (j + 1);
                    boundaries[columnCount * i + j] = new Boundary2D(columnLow, columnHigh, rowLow, rowHigh);
                }
            }

            return boundaries;
        }

        /// <summary>
        /// aabb 碰撞检查
        /// </summary>
        /// <returns></returns>
        public bool IsCollided(Boundary2D boundary)
        {
            return (this.XLow <= boundary.XHigh && this.XHigh >= boundary.XLow)
                   && (this.YLow <= boundary.YHigh && this.YHigh >= boundary.YLow);
        }

        public bool IsCrossed(Boundary2D boundary)
        {
            return (this.XLow < boundary.XHigh && this.XHigh > boundary.XLow)
                   && (this.YLow < boundary.YHigh && this.YHigh > boundary.YLow);
        }


        public override string ToString()
        {
            return $"X:{XLow}->{XHigh},Y:{YLow}->{YHigh}";
        }
    }
}