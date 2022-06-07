namespace RLP.Chart.Interface.Abstraction
{
    public interface IPoint2D : IGeometry
    {
        float X { get; }
        float Y { get; }
    }

    public interface IPoint3D : IPoint2D
    {
        float Z { get; }
    }
}