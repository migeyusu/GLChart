using System;
using System.Windows;
using OpenTK.Input;
using RLP.Chart.Interface;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL
{
    /// <summary>
    /// 比例尺
    /// </summary>
    public struct WindowsToGlRecMapping
    {
        private Rect _pixelRect; //像素区域

        private readonly double _xScaleRatio;
        private readonly double _yScaleRatio;
        private readonly ScrollRange _xScrollRange;
        private readonly ScrollRange _yScrollRange;

        public double XScaleRatio => _xScaleRatio;

        public double YScaleRatio => _yScaleRatio;

        public WindowsToGlRecMapping(ScrollRange xScrollRange, ScrollRange yScrollRange, Rect pixelRect)
        {
            this._xScrollRange = xScrollRange;
            this._yScrollRange = yScrollRange;
            this._pixelRect = pixelRect;
            _xScaleRatio = xScrollRange.Range / pixelRect.Width;
            _yScaleRatio = yScrollRange.Range / pixelRect.Height;
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
                throw new ArgumentException(nameof(newPixelRect));
            }

            newPixelRect.Intersect(_pixelRect);
            //wpf的窗体的y轴方向是反转的，而y轴坐标start也从bottom开始
            var bottom = _yScrollRange.Start + (_pixelRect.Bottom - newPixelRect.Bottom) * YScaleRatio;
            var top = bottom + newPixelRect.Height * YScaleRatio;
            var left = _xScrollRange.Start + (newPixelRect.Left - _pixelRect.Left) * XScaleRatio;
            var right = left + newPixelRect.Width * XScaleRatio;
            yRange = new ScrollRange(bottom, top);
            xRange = new ScrollRange(left, right);
        }

        public System.Windows.Point MapWinPoint(Point2D point)
        {
            var x = (point.X - _xScrollRange.Start) / XScaleRatio;
            var y = (point.Y - _yScrollRange.Start) / YScaleRatio;
            return new System.Windows.Point(x, _pixelRect.Height - y);
        }

        public Point2D MapGlPoint(System.Windows.Point winPoint)
        {
            winPoint.Y = _pixelRect.Height - winPoint.Y;
            return new Point2D((float) (XScaleRatio * winPoint.X + _xScrollRange.Start),
                (float) (YScaleRatio * winPoint.Y + _yScrollRange.Start));
        }
    }
}