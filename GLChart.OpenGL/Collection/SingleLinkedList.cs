using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RLP.Chart.OpenGL.Collection
{
    /// <summary>
    /// 单链表的简单实现
    /// </summary>
    public class SingleLinkedList<T> : IEnumerable<T>
    {
        /// <summary>
        /// 头节点
        /// </summary>
        private readonly SingleLinkedNode<T> _head;

        /// <summary>
        /// 尾节点
        /// </summary>
        private SingleLinkedNode<T> _tail;

        public SingleLinkedList()
        {
            _head = new EmptyLinkedNode();
            _tail = _head;
        }

        /// <summary>
        /// 头节点
        /// </summary>
        public SingleLinkedNode<T> Head => _head;

        /// <summary>
        /// 尾节点
        /// </summary>
        public SingleLinkedNode<T> Tail => _tail;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            return _head.Next == null;
        }

        public void Clear()
        {
            _head.Next = null;
            _tail = _head;
        }

        public void Append(T data)
        {
            var linkedNode = new SingleLinkedNode<T>() { Data = data };
            _tail.Next = linkedNode;
            _tail = linkedNode;
        }


        public void Remove(T t, EqualityComparer<T> comparer = null)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            Remove(arg => comparer.Equals(arg, t));
        }

        public void Remove(Func<T, bool> removeFunc)
        {
            if (IsEmpty())
            {
                return;
            }

            var prev = _head;
            var node = _head.Next;
            while (node != null)
            {
                if (removeFunc.Invoke(node.Data))
                {
                    var next = node.Next;
                    if (next == null)
                    {
                        prev.Next = null;
                        _tail = prev;
                    }
                    else
                    {
                        prev.Next = next;
                    }

                    return;
                }

                prev = node;
                node = node.Next;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public SingleLinkedNode<T> Find(T t,EqualityComparer<T> comparer=null)
        {
            if (comparer==null)
            {
                comparer=EqualityComparer<T>.Default;
            }
            return Find((arg => comparer.Equals(t, arg)));
        }

        public SingleLinkedNode<T> Find(Func<T, bool> findFunc)
        {
            if (IsEmpty())
            {
                return null;
            }

            var node = _head.Next;
            while (node != null)
            {
                if (findFunc.Invoke(node.Data))
                {
                    return node;
                }

                node = node.Next;
            }

            return default;
        }


        /// <summary>
        /// 特殊的结点，没有具体数据
        /// </summary>
        public class EmptyLinkedNode : SingleLinkedNode<T>
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SingleLinkedListEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class SingleLinkedListEnumerator : IEnumerator<T>
        {
            private readonly SingleLinkedList<T> _list;

            public SingleLinkedListEnumerator(SingleLinkedList<T> list)
            {
                _list = list ?? throw new ArgumentNullException(nameof(list));
                _currentNode = _list._head;
            }

            private SingleLinkedNode<T> _currentNode;

            public bool MoveNext()
            {
                if (_currentNode.Next == null)
                {
                    return false;
                }

                _currentNode = _currentNode.Next;
                return true;
            }

            public void Reset()
            {
                _currentNode = _list._head;
            }

            object IEnumerator.Current => this.Current;

            public T Current => _currentNode.Data;

            public void Dispose()
            {
            }
        }
    }


    /// <summary>
    /// 单链表节点
    /// </summary>
    public class SingleLinkedNode<T>
    {
        public T Data { get; set; }

        public SingleLinkedNode<T> Next { get; set; }
    }
}