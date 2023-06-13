using GLChart.Interface.Abstraction;

namespace GLChart.OpenTK.Renderer
{
    public struct Channel : IChannel
    {
        public IPoint3D[] Points { get; set; }

        public Channel(IPoint3D[] point3Ds)
        {
            this.Points = point3Ds;
        }
    }
}