using RLP.Chart.Interface;

namespace RLP.Chart.OpenGL.Interaction
{
    public static class MouseWheelScaleExtension
    {
        public static ScrollRange Scale(this System.Windows.Input.MouseWheelEventArgs e,
            double selectionStart, double selectionEnd)
        {
            var d = e.Delta / 120;
            if (d > 9)
            {
                d = 9;
            }

            var ratio = 1 - d * 0.1;
            var center = (selectionEnd + selectionStart) / 2;
            var start = (selectionStart - center) * ratio + center;
            var end = (selectionEnd - center) * ratio + center;
            return new ScrollRange(start, end);
        }

        public static ScrollRange Scale(this System.Windows.Input.MouseWheelEventArgs e,
            ScrollRange range)
        {
            return e.Scale(range.Start, range.End);
        }

        public static Region2D Scale(this System.Windows.Input.MouseWheelEventArgs e, Region2D region)
        {
            var d = e.Delta / 120;
            if (d > 9)
            {
                d = 9;
            }

            var ratio = 1 - d * 0.1;
            var regionLeft = region.Left;
            var regionRight = region.Right;
            var regionTop = region.Top;
            var regionBottom = region.Bottom;
            var horizontalCenter = (regionLeft + regionRight) / 2;
            var newRegionLeft = (regionLeft - horizontalCenter) * ratio + horizontalCenter;
            var newRegionRight = (regionRight - horizontalCenter) * ratio + horizontalCenter;
            var verticalCenter = (regionTop + regionBottom) / 2;
            var newRegionBottom = (regionBottom - verticalCenter) * ratio + verticalCenter;
            var newRegionTop = (regionTop - verticalCenter) * ratio + verticalCenter;
            return new Region2D(newRegionTop, newRegionBottom, newRegionLeft, newRegionRight);
        }
    }
}