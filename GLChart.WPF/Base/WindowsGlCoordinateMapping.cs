using System;
using System.Windows;
using GLChart.WPF.Render;

namespace GLChart.WPF.Base
{
    //todo：性能优化
    /// <summary>
    /// windows 和 opengl的空间映射
    /// </summary>
    public struct WindowsGlCoordinateMapping
    {
        private Rect _pixelRect; //像素区域

        private readonly double _xScaleRatio;

        private readonly double _yScaleRatio;

        private readonly Region2D _scrollRegion;

        public double XScaleRatio => _xScaleRatio;

        public double YScaleRatio => _yScaleRatio;

        public WindowsGlCoordinateMapping(Region2D region2D, Rect pixelRect)
        {
            _scrollRegion = region2D;
            this._pixelRect = pixelRect;
            _xScaleRatio = region2D.XExtend / pixelRect.Width;
            _yScaleRatio = region2D.YExtend / pixelRect.Height;
        }

        public double GetXOffset(double xPixelDistance)
        {
            return xPixelDistance * _xScaleRatio;
        }

        public double GetYOffset(double yPixelDistance)
        {
            return yPixelDistance * _yScaleRatio;
        }

        public void ScaleByRect(Rect newPixelRect, out ScrollRange xRange,
            out ScrollRange yRange, bool validate = false)
        {
            if (!_pixelRect.Contains(newPixelRect) && validate)
            {
                throw new ArgumentException(null, nameof(newPixelRect));
            }

            newPixelRect.Intersect(_pixelRect);
            //wpf的窗体的y轴方向是反转的，而y轴坐标start也从bottom开始
            var bottom = _scrollRegion.Bottom + (_pixelRect.Bottom - newPixelRect.Bottom) * YScaleRatio;
            var top = bottom + newPixelRect.Height * YScaleRatio;
            var left = _scrollRegion.Left + (newPixelRect.Left - _pixelRect.Left) * XScaleRatio;
            var right = left + newPixelRect.Width * XScaleRatio;
            yRange = new ScrollRange(bottom, top);
            xRange = new ScrollRange(left, right);
        }

        public Point GetWindowsPointByGlPoint(Point2D point)
        {
            var x = (point.X - _scrollRegion.Left) / XScaleRatio;
            var y = (point.Y - _scrollRegion.Bottom) / YScaleRatio;
            return new Point(x, _pixelRect.Height - y);
        }

        public Point2D GetGlPointByWindowsPoint(Point winPoint)
        {
            winPoint.Y = _pixelRect.Height - winPoint.Y;
            return new Point2D((float)(XScaleRatio * winPoint.X + _scrollRegion.Left),
                (float)(YScaleRatio * winPoint.Y + _scrollRegion.Bottom));
        }
    }
}