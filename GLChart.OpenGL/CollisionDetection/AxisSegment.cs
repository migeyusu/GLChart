using System;
using GLChart.Interface;

namespace GLChart.Core.CollisionDetection
{
    public readonly struct AxisSegment
    {
        public AxisSegmentEndPoint Start { get; }

        public AxisSegmentEndPoint End { get; }

        public float Length { get; }

        public AxisSegment(AxisSegmentEndPoint start, AxisSegmentEndPoint end)
        {
            if (start.Value > end.Value)
            {
                throw new ArgumentException("Can't represent a segment!");
            }

            if (start == end && !start.IsClose)
            {
                throw new ArgumentException("Can't represent a segment!");
            }

            Start = start;
            End = end;
            this.Length = End.Value - start.Value;
        }

        public ScrollRange Range
        {
            get { return new ScrollRange(this.Start.Value, this.End.Value); }
        }

        public bool Contains(AxisSegment segment)
        {
            return segment == this || segment < this;
        }

        public bool Contains(float value)
        {
            if (Start.Value < value && End.Value > value)
            {
                return true;
            }

            if (Start.IsClose && Start.Value.Equals(value))
            {
                return true;
            }

            if (End.IsClose && End.Value.Equals(value))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 通过步长得到刻度索引，从低到高
        /// </summary>
        /// <returns></returns>
        public bool TryGetTicks(float step, float value, out int index)
        {
            index = -1;
            if (!Contains(value))
            {
                return false;
            }

            index = (int)Math.Floor((value - Start.Value) / step);
            return true;
        }

        /// <summary>
        /// 表示轴区间范围的包含关系，也可以理解为二维向量的比较。
        /// <para>如果左操作数，或左向量更大，右向量可以完全被左向量包含</para>
        /// <para>如果范围不重合则不适用于包含关系，也没有大小之分了</para>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(AxisSegment left, AxisSegment right)
        {
            if (left.Start < right.Start)
            {
                return false;
            }

            if (left.Start > right.Start)
            {
                return left.End <= right.End;
            }

            return left.End < right.End;
        }

        public static bool operator <(AxisSegment left, AxisSegment right)
        {
            if (left.Start > right.Start)
            {
                return false;
            }

            if (left.Start < right.Start)
            {
                return left.End >= right.End;
            }

            return left.End > right.End;
        }

        public bool Equals(AxisSegment other)
        {
            return Start == other.Start && End == other.End;
        }

        public static bool operator ==(AxisSegment left, AxisSegment right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AxisSegment left, AxisSegment right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is AxisSegment other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }
    }
}