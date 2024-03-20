using System;
using System.ComponentModel;

namespace GLChart.WPF.Base
{
    /// <summary>
    /// 范围
    /// </summary>
    [TypeConverter(typeof(ScrollRangeConverter))]
    public readonly struct ScrollRange
    {
        public static ScrollRange Empty = new ScrollRange(0, 0);

        [DefaultValue(0)] public double Start { get; }

        [DefaultValue(100)] public double End { get; }

        public double Range { get; }

        public ScrollRange(double start, double end)
        {
            if (end < start) //允许相等
            {
                throw new ArgumentException($"{nameof(end)} can't lower than {nameof(start)}");
            }

            Start = start;
            End = end;
            Range = end - start;
        }

        public ScrollRange WithEnd(double end)
        {
            return new ScrollRange(this.Start, end);
        }

        public ScrollRange WithStart(double start)
        {
            return new ScrollRange(start, this.End);
        }

        public bool IsEmpty()
        {
            return Range <= double.Epsilon;
        }

        public ScrollRange OffsetNew(double offset)
        {
            return new ScrollRange(Start + offset, End + offset);
        }

        public ScrollRange Merge(ScrollRange range)
        {
            var rangeStart = range.Start;
            var rangeEnd = range.End;
            var start = rangeStart < this.Start ? rangeStart : this.Start;
            var end = rangeEnd > this.End ? rangeEnd : this.End;
            return new ScrollRange(start, end);
        }

        public bool Equals(ScrollRange other)
        {
            return Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object? obj)
        {
            return obj is ScrollRange other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"Start:{Start},End:{End}";
        }
    }
}