using GLChart.Interface;

namespace GLChart.OpenTK.Renderer
{
    public struct Region3D
    {
        public float Top;

        public float Bottom;

        public float Left;

        public float Right;

        public float Front;

        public float Back;

        public float Height => this.Top - this.Bottom;

        public float Width => this.Right - this.Left;

        public ScrollRange YRange => new ScrollRange(this.Bottom, this.Top);

        public ScrollRange XRange => new ScrollRange(this.Left, this.Right);

        public ScrollRange ZRange => new ScrollRange(this.Back, this.Front);

        
    }
}