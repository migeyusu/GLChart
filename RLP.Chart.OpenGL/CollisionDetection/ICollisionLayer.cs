using System;
using System.Collections.Generic;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.CollisionDetection
{
    /// <summary>
    /// 分层碰撞，实现弹性的碰撞空间以最大化内存效率
    /// </summary>
    public interface ICollisionLayer
    {
        Guid Id { get; }

        bool TrySearch(Geometry2D geometry, out Node point);

        void AddNode(IPoint2D point);

        void AddNodes(IEnumerable<IPoint2D> points);

        void ResetWithNode(IPoint2D point);

        void ResetWithNodes(IEnumerable<IPoint2D> nodes);

        void ClearNodes();
    }
}