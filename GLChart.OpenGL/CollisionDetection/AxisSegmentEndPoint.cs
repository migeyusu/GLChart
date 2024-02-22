

namespace GLChart.Core.CollisionDetection
{
    public readonly struct AxisSegmentEndPoint
    {
        public float Value { get; }

        public bool IsClose { get; }

        public AxisSegmentEndPoint(float value, bool isClose)
        {
            IsClose = isClose;
            Value = value;
        }

        /// <summary>
        /// 指轴数值范围（包含的点数量）大小比较，默认轴向右正向；
        /// 所以，当左操作数更靠近左边时，表示的范围更大
        /// 当左操作数封闭时，右操作数如果不封闭，小于等于即可，此时左操作数永远多容纳一个值。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <=(AxisSegmentEndPoint left, AxisSegmentEndPoint right)
        {
            return left == right || left < right;
        }

        /// <summary>
        /// 指轴数值范围（包含的点数量）大小比较，默认轴向右正向；
        /// 所以，当左操作数更靠近左边时，表示的范围更大
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(AxisSegmentEndPoint left, AxisSegmentEndPoint right)
        {
            return left == right || left > right;
        }

        /// <summary>
        /// 指轴数值范围（包含的点数量）大小比较，默认轴向右正向；
        /// 所以，当左操作数更靠近左边时，表示的范围更大
        /// 当左操作数封闭时，右操作数如果不封闭，小于等于即可，此时左操作数永远多容纳一个值。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(AxisSegmentEndPoint left, AxisSegmentEndPoint right)
        {
            if (left.IsClose)
            {
                if (right.IsClose)
                {
                    return left.Value < right.Value;
                }

                return left.Value <= right.Value;
            }

            return left.Value < right.Value;
        }

        /// <summary>
        /// 指轴数值范围（包含的点数量）大小比较，默认轴向右正向；
        /// 所以，当左操作数更靠近左边时，表示的范围更大
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(AxisSegmentEndPoint left, AxisSegmentEndPoint right)
        {
            if (left == right)
            {
                return false;
            }

            return !(left > right);
        }

        /// <summary>
        /// 指轴数值范围（包含的点数量）大小比较，默认轴向右正向；
        /// 所以，当左操作数更靠近左边时，表示的范围更大
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(AxisSegmentEndPoint left, AxisSegmentEndPoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 指轴数值范围（包含的点数量）大小比较，默认轴向右正向；
        /// 所以，当左操作数更靠近左边时，表示的范围更大
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(AxisSegmentEndPoint left, AxisSegmentEndPoint right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 指轴数值范围（包含的点数量）大小比较，默认轴向右正向；
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AxisSegmentEndPoint other)
        {
            return Value.Equals(other.Value) && IsClose == other.IsClose;
        }

        /// <summary>
        /// 指轴数值范围（包含的点数量）大小比较，默认轴向右正向；
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is AxisSegmentEndPoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value.GetHashCode() * 397) ^ IsClose.GetHashCode();
            }
        }
    }
}