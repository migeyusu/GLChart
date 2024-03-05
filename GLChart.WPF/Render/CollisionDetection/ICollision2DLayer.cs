using GLChart.WPF.Base;

namespace GLChart.WPF.Render.CollisionDetection;

/// <summary>
/// 在二维平面上执行碰撞检测的层，给每个类型的集合分配独立的碰撞检测层以最大化内存效率
/// </summary>
public interface ICollision2DLayer
{
    /// <summary>
    /// 可以接收鼠标点并获得最近的几何体
    /// </summary>
    /// <param name="mouse">鼠标光点</param>
    /// <param name="data"></param>
    /// <returns></returns>
    bool TrySearch(MouseCollisionEllipse mouse, out IGeometry2D? data);
}