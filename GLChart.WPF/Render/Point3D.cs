using GLChart.WPF.Base;

namespace GLChart.WPF.Render
{
    public readonly struct Point3D : IPoint3D
    {
        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}