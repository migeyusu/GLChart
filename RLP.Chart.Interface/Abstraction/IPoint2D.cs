namespace RLP.Chart.Interface.Abstraction
{
    public interface IPoint2D: IGeometry
    {
        float X { get; }
        float Y { get; }
    }

    public interface IPoint3D:IGeometry
    {
        float X { get; }
        float Y { get; }
        float Z { get; }
    }
}