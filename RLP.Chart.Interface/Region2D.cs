using System.Security.Cryptography;

namespace RLP.Chart.Interface
{
    /// <summary>
    /// 表示图标坐标系区域，和windows的<see cref="Rect"/>相反：top为高点，bottom为低点
    /// </summary>
    public struct Region2D
    {
        public double Top;

        public double Bottom;

        public double Left;

        public double Right;

        public double Height => this.Top - this.Bottom;

        public double Width => this.Right - this.Left;

        public ScrollRange YRange => new ScrollRange(this.Bottom, this.Top);

        public ScrollRange XRange => new ScrollRange(this.Left, this.Right);

        public Region2D(double top, double bottom, double left, double right)
        {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
        }

        public Region2D(ScrollRange xRange, ScrollRange yRange)
        {
            this.Top = yRange.End;
            this.Bottom = yRange.Start;
            this.Left = xRange.Start;
            this.Right = xRange.End;
        }

        public Region2D ChangeXRange(ScrollRange range)
        {
            this.Left = range.Start;
            this.Right = range.End;
            return this;
        }

        public Region2D ChangeYRange(ScrollRange range)
        {
            this.Top = range.End;
            this.Bottom = range.Start;
            return this;
        }

        public Region2D SetTop(double top)
        {
            this.Top = top;
            return this;
        }

        public Region2D WithTop(double top)
        {
            return new Region2D(top, this.Bottom, this.Left, this.Right);
        }

        public Region2D SetBottom(double bottom)
        {
            this.Bottom = bottom;
            return this;
        }

        public Region2D SetLeft(double left)
        {
            this.Left = left;
            return this;
        }

        public Region2D SetRight(double right)
        {
            this.Right = right;
            return this;
        }

        public bool IsXAxisChanged(Region2D newRegion)
        {
            return !Left.Equals(newRegion.Left) || !Right.Equals(newRegion.Right);
        }

        public bool IsYAxisChanged(Region2D newRegion)
        {
            return !Bottom.Equals(newRegion.Bottom) || !Top.Equals(newRegion.Top);
        }

        public bool IsEmpty()
        {
            if (Height == 0 || Width == 0)
            {
                return true;
            }

            return false;
        }

        public void Offset(double x, double y)
        {
            Left += x;
            Right += x;
            Bottom += y;
            Top += y;
        }

        public Region2D CreateOffset(double x, double y)
        {
            return new Region2D(Top + y, Bottom + y, Left + x, Right + x);
        }

        public override string ToString()
        {
            return $"Left:{Left},Top:{Top},Bottom:{Bottom},Right:{Right}";
        }
    }
}