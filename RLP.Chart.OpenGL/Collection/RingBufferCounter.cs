using System;
using System.Collections.Generic;

namespace RLP.Chart.OpenGL.Collection
{
    /// <summary>
    /// 针对数组类型的环形缓冲索引指示器，和缓冲不绑定
    /// </summary>
    public class RingBufferCounter
    {
        private readonly int _capacity;

        public int Capacity => _capacity;

        private int _head = -1, _tail = 0;

        public int Head => _head;

        public int Tail => _tail;

        /// <summary>
        /// 表示缓冲内的<see cref="Head"/>和<see cref="Tail"/>的结构体
        /// </summary>
        public Region ContentRegion
        {
            get { return new Region() {Head = _head, Tail = _tail}; }
        }

        /// <summary>
        /// 表示缓冲有效区域的的组合，<see cref="Head"/>大于<see cref="Tail"/>
        /// </summary>
        public IEnumerable<Region> ContiguousRegions
        {
            get
            {
                if (_head < 0)
                {
                    yield break;
                }

                if (_tail > 0)
                {
                    yield return new Region() {Head = Capacity - 1, Tail = _tail};
                    yield return new Region() {Head = Head, Tail = 0};
                }
                else
                {
                    yield return new Region() {Head = Head, Tail = _tail};
                }
            }
        }

        public int Length
        {
            get
            {
                if (_head < 0)
                {
                    return 0;
                }

                if (_tail > 0)
                {
                    return _capacity;
                }

                return _head - _tail + 1;
            }
        }

        public RingBufferCounter(int length)
        {
            if (length < 1)
            {
                throw new ArgumentNullException(nameof(length));
            }

            _capacity = length;
        }

        /// <summary>
        /// 在集合末尾增加1个
        /// </summary>
        /// <returns>current head</returns>
        public int Increment()
        {
            _head++;
            if (_head >= _capacity)
            {
                _head = 0;
                _tail = 1;
            }
            else if (_tail > 0)
            {
                _tail++;
                if (_tail >= _capacity)
                {
                    _tail = 0;
                }
            }

            return _head;
        }

        /// <summary>
        /// 在集合末尾附加指定长度
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Add(uint length)
        {
            if (length < 1)
            {
                return _head;
            }

            if (length == 1)
            {
                return Increment();
            }

            var virtualHead = _head + length;
            if (virtualHead < _capacity)
            {
                _head = (int) virtualHead;
                if (_tail > 0)
                {
                    _tail = _head + 1;
                    if (_tail == _capacity)
                    {
                        _tail = 0;
                    }
                }
            }
            else
            {
                _head = (int) (virtualHead % _capacity);
                _tail = _head + 1;
                if (_tail == _capacity)
                {
                    _tail = 0;
                }
            }

            return _head;
        }

        public void Reset()
        {
            _head = -1;
            _tail = 0;
        }

        /// <summary>
        /// 增加指定长度缓冲，并返回需要更新的物理区域（标记为“脏”区间）
        /// 按顺序返回脏区域
        /// </summary>
        /// <param name="length">脏区域的长度</param>
        /// <returns>dirt region, need to update</returns>
        public IEnumerable<Region> AddDifference(uint length)
        {
            if (length < 1)
            {
                yield break;
            }

            var oldHead = this.Head;
            var newHead = Add(length);
            if (length >= Capacity)
            {
                //完全更新
                foreach (var contiguousRegion in ContiguousRegions)
                {
                    yield return contiguousRegion;
                }
                // yield return new Region() {Head = Capacity - 1, Tail = 0};
            }
            else
            {
                if (newHead > oldHead)
                {
                    yield return new Region() {Head = newHead, Tail = oldHead + 1};
                }
                else
                {
                    if (oldHead < Capacity - 1)
                    {
                        yield return new Region() {Head = Capacity - 1, Tail = oldHead + 1};
                    }
                    yield return new Region() { Head = newHead, Tail = 0 };
                }
            }
        }

        public int GetPhysicalIndex(int index)
        {
            if (index > _capacity || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (_head < 0)
            {
                throw new NotSupportedException("Can't get any elements as buffer is empty!");
            }

            var arrayIndex = _tail + index;
            if (_tail == 0 && arrayIndex > _head)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            arrayIndex %= _capacity;
            return arrayIndex;
        }

        /// <summary>
        /// 指示环形缓冲（数组）的特定区域，当用来表示环形缓冲时，tail为尾结点索引，head为头结点索引，所以可能出现{0,0}
        /// <para>当用来表示更新后的"dirty region"时，<see cref="Region.Tail"/>表示更新区域的低位索引，
        /// <see cref="Region.Head"/>表示更新区域的高位索引。</para>
        /// <para>例如 {0,0}表示需要更新0索引，{0,1} 表示需要更新0和1两个索引位</para>
        /// </summary>
        public struct Region
        {
            public int Head;
            public int Tail;

            public int Length
            {
                get
                {
                    if (Tail > Head)
                    {
                        return 0;
                    }

                    return Head - Tail + 1;
                }
            }
        }
    }
}