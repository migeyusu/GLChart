using System.Windows.Media;
using GLChart.WPF.Base;
using GLChart.WPF.Render.CollisionDetection;

namespace GLChart.WPF.UIComponent;

/// <summary>
/// 表示绘图的系列
/// </summary>
public interface ISeries2D : IGeometry2D
{
    string Title { get; set; }

    Color Color { get; set; }
    
    internal ICollision2DLayer CollisionLayer { get; }
}

/// <summary>
/// 表示绘图的系列
/// </summary>
public interface ISeries2D<T> : ISeries2D, IGeometryCollection<T> where T : IGeometry2D
{
}