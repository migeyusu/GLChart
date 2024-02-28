using System;

namespace GLChart.WPF.Render.Allocation
{
    public static class FloatPointCompareExtension
    {
        public static bool Same(this double left, double right, double epsilon = double.Epsilon)
        {
            if (double.IsNaN(left) || double.IsNaN(right))
            {
                return false;
            }

            var isLeftInfinity = double.IsInfinity(left);
            var isRightInfinity = double.IsInfinity(right);
            if (isLeftInfinity && isRightInfinity)
            {
                return true;
            }

            if (isLeftInfinity ^ isRightInfinity)
            {
                return false;
            }

            var absA = Math.Abs(left);
            var absB = Math.Abs(right);
            var diff = Math.Abs(left - right);
            if (left.Equals(right))
            {
                return true;
            }

            if (absA == 0 || absB == 0 || absA + absB < double.Epsilon)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * double.Epsilon);
            }

            // use relative error
            return diff / (absA + absB) < epsilon;
        }

        public static bool Same(this float left, float right, float epsilon = float.Epsilon)
        {
            if (double.IsNaN(left) || double.IsNaN(right))
            {
                return false;
            }

            var isLeftInfinity = double.IsInfinity(left);
            var isRightInfinity = double.IsInfinity(right);
            if (isLeftInfinity && isRightInfinity)
            {
                return true;
            }

            if (isLeftInfinity ^ isRightInfinity)
            {
                return false;
            }
            var absA = Math.Abs(left);
            var absB = Math.Abs(right);
            var diff = Math.Abs(left - right);
            if (left.Equals(right))
            {
                return true;
            }

            if (absA == 0 || absB == 0 || absA + absB < float.Epsilon)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * float.Epsilon);
            }

            // use relative error
            return diff / (absA + absB) < epsilon;
        }
    }
}