using System;

namespace RLP.Chart.Interface.Abstraction
{
    public interface ILineSeries : ISeries2D, ILine, IGeometryCollection<IPoint2D>
    {
        Guid Id { get; }

        string Title { get; set; }

        bool Visible { get; set; }
    }
}