using System;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.CollisionDetection
{
    /// <summary>
    /// 二维点碰撞层，应用分层碰撞的思想，给每个类型的集合体分配独立的碰撞检测层，
    /// 实现灵活的碰撞空间以最大化内存效率
    /// </summary>
    public interface IPoint2DCollisionLayer : IGeometryCollection<IPoint2D>
    {
        Guid Id { get; }

        bool TrySearch(Geometry2D geometry, out Node point);
    }
}