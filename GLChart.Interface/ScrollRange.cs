﻿using System;
using System.ComponentModel;

namespace GLChart.Interface
{
    /// <summary>
    /// 滚动范围
    /// </summary>
    public readonly struct ScrollRange
    {
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

        public override bool Equals(object obj)
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