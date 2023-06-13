using System;
using GLChart.Interface.Abstraction;

namespace GLChart.OpenTK.CollisionDetection
{
    /// <summary>
    /// 二维点碰撞层，给每个类型的集合分配独立的碰撞检测层以最大化内存效率
    /// </summary>
    public interface ICollisionPoint2D : IGeometryCollection<IPoint2D>
    {
        /// <summary>
        /// used for finding
        /// </summary>
        Guid Id { get; }

        bool TrySearch(ICollisionGeometry2D geometry, out Node point);
    }
}