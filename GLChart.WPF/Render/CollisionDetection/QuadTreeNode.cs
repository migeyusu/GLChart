using System.Collections.Generic;

namespace GLChart.WPF.Render.CollisionDetection
{
    /*为了配合碰撞，四叉树可以限定精度，可以在插入时限制大小，也可以在搜索时限定大小
     后者的性能更差，但灵活度更好，当前采用后者*/

    //递归方式性能已经满足，如果需要应改为循环

    /// <summary>
    /// 四叉树节点
    /// </summary>
    public class QuadTreeNode
    {
        public Boundary2D Boundary { get; }
        public Point2DNode? Data { get; private set; }
        public QuadTreeNode NorthWest { get; private set; }
        public QuadTreeNode NorthEast { get; private set; }
        public QuadTreeNode SouthWest { get; private set; }
        public QuadTreeNode SouthEast { get; private set; }

        private QuadTreeNode[] _nodes;

        public QuadTreeNode(Boundary2D boundary)
        {
            this.Boundary = boundary;
        }

        /// <summary>
        /// 最终实现每个节点分配一个数据
        /// </summary>
        /// <param name="node"></param>
        public void Insert(Point2DNode node)
        {
            if (!node.Point.IsLocatedIn(this.Boundary))
            {
                return;
            }

            if (_nodes != null) //已经分块，本节点不再承载任何数据
            {
                foreach (var quadTreeNode in _nodes)
                {
                    quadTreeNode.Insert(node);
                }

                return;
            }

            if (!Data.HasValue) //未分块且空
            {
                Data = node;
                return;
            }

            //未分块，且已有数据
            if (Data.Value.Point.Equals(node.Point))
            {
                Data = node;
                return;
            }

            //四分块然后插入数据两次
            var boundaries = Boundary.Divide(2);
            NorthWest = new QuadTreeNode(boundaries[0]);
            NorthEast = new QuadTreeNode(boundaries[1]);
            SouthWest = new QuadTreeNode(boundaries[2]);
            SouthEast = new QuadTreeNode(boundaries[3]);
            _nodes = new[] { NorthWest, NorthEast, SouthWest, SouthEast };

            foreach (var quadTreeNode in _nodes)
            {
                quadTreeNode.Insert(node);
            }

            foreach (var treeNode in _nodes)
            {
                var nodeData = Data.Value;
                treeNode.Insert(nodeData);
            }

            this.Data = null;
        }

        public bool TrySearch(Point2D point, out Point2DNode data)
        {
            if (!point.IsLocatedIn(Boundary))
            {
                data = default(Point2DNode);
                return false;
            }

            if (this.Data.HasValue)
            {
                data = this.Data.Value;
                return true;
            }

            foreach (var quadTreeNode in _nodes)
            {
                if (quadTreeNode.TrySearch(point, out data))
                {
                    return true;
                }
            }

            //理论上不会出现的情况
            data = default(Point2DNode);
            return false;
        }

        /// <summary>
        /// 搜索点位落入的区间且存在值
        /// </summary>
        /// <param name="point"></param>
        public QuadTreeNode Search(Point2D point)
        {
            if (!point.IsLocatedIn(Boundary))
            {
                return null;
            }

            if (_nodes != null)
            {
                foreach (var node in _nodes)
                {
                    var treeNode = node.Search(point);
                    if (treeNode != null)
                    {
                        return treeNode;
                    }
                }
            }

            return this.Data.HasValue ? this : null;
        }

        /// <summary>
        /// 精度搜索，默认以大者为限制
        /// </summary>
        /// <param name="point"></param>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        public IEnumerable<QuadTreeNode> Search(Point2D point, float accuracy)
        {
            if (!point.IsLocatedIn(Boundary))
            {
                yield break;
            }

            if (this.Boundary.XSpan <= accuracy || this.Boundary.YSpan <= accuracy)
            {
                foreach (var quadTreeNode in this.TraverseNotEmpty())
                {
                    yield return quadTreeNode;
                }

                yield break;
            }

            if (_nodes != null)
            {
                foreach (var node in _nodes)
                {
                    foreach (var quadTreeNode in node.Search(point, accuracy))
                    {
                        yield return quadTreeNode;
                    }
                }

                yield break;
            }

            if (Data.HasValue)
            {
                yield return this;
            }
        }

        /// <summary>
        /// 只遍历有数据的节点
        /// </summary>
        /// <returns></returns>
        public IEnumerable<QuadTreeNode> TraverseNotEmpty()
        {
            if (_nodes == null)
            {
                if (Data.HasValue)
                {
                    yield return this;
                }
            }
            else
            {
                foreach (var node in _nodes)
                {
                    foreach (var quadTreeNode in node.TraverseNotEmpty())
                    {
                        yield return quadTreeNode;
                    }
                }
            }
        }

        /// <summary>
        /// 遍历，直到没有子叶
        /// </summary>
        /// <returns></returns>
        public IEnumerable<QuadTreeNode> Traverse()
        {
            if (_nodes == null)
            {
                yield return this;
            }
            else
            {
                foreach (var node in _nodes)
                {
                    foreach (var quadTreeNode in node.Traverse())
                    {
                        yield return quadTreeNode;
                    }
                }
            }
        }
    }
}