using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GLChart.Core.Collection
{
    /// <summary>
    /// thread-safe,vector instruction interoperable and flexible ring render ring array
    /// </summary>
    public class ThreadSafeRingBuffer<T> : IList<T>, IDisposable
    {
        /// <summary>
        /// 由于环形缓冲的特殊性，当写入指定索引或对长度修改时触发写锁，只有读取指定索引使用读锁
        /// </summary>
        private readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

        public int Capacity => _counter.Capacity;

        public int Count => _counter.Length;

        public bool IsReadOnly => false;

        public int Head => _counter.Head;

        public int Tail => _counter.Tail;

        private readonly IList<T> _buffer;

        private readonly RingBufferCounter _counter;

        public ThreadSafeRingBuffer(int length, IList<T> buffer)
        {
            this._buffer = buffer;
            var bufferCount = buffer.Count;
            while (bufferCount < length)
            {
                buffer.Add(default);
                bufferCount++;
            }

            this._counter = new RingBufferCounter(length);
        }


        public IList<T> ContentBuffer => _buffer;

        public T GetLast()
        {
            _readerWriterLockSlim.EnterReadLock();
            try
            {
                if (_buffer.Count == 0)
                {
                    return default;
                }

                return _buffer[_counter.Tail];
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        public T GetFirst()
        {
            _readerWriterLockSlim.EnterReadLock();

            try
            {
                if (_buffer.Count == 0)
                {
                    return default;
                }

                return _buffer[_counter.Head];
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return IndexOf(item, default);
        }


        public int IndexOf(T item, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            _readerWriterLockSlim.EnterReadLock();
            try
            {
                for (int i = 0; i < _counter.Length; i++)
                {
                    var physicalIndex = _counter.GetPhysicalIndex(i);
                    if (comparer.Equals(_buffer[physicalIndex], item))
                    {
                        return i;
                    }
                }

                return -1;
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        public void Insert(int index, T item)
        {
            this.Insert(index, item, true);
        }

        public void Insert(int index, T item, bool checkCapacity)
        {
            if (index > Capacity && checkCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Range out of {Capacity}");
            }

            index %= Capacity;
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                var counterLength = _counter.Length;
                if (counterLength <= index)
                {
                    var add = index - counterLength + 1;
                    _counter.Add((uint)add);
                }

                var physicalIndex = _counter.GetPhysicalIndex(index);
                _buffer[physicalIndex] = item;
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void RemoveAt(int index)
        {
            if (Capacity <= index)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Capacity is {Capacity}!");
            }

            if (_counter.Length <= index)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Length is {Count}");
            }

            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                var physicalIndex = _counter.GetPhysicalIndex(index);
                _buffer[physicalIndex] = default;
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public T this[int index]
        {
            get
            {
                _readerWriterLockSlim.EnterReadLock();
                try
                {
                    if (_counter.Length <= index)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index), $"Length is {Count}");
                    }

                    var i = _counter.GetPhysicalIndex(index);
                    return _buffer[i];
                }
                finally
                {
                    _readerWriterLockSlim.ExitReadLock();
                }
            }
            set
            {
                _readerWriterLockSlim.EnterWriteLock();
                try
                {
                    if (_counter.Length <= index)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index), $"Length is {Count}");
                    }

                    var i = _counter.GetPhysicalIndex(index);
                    _buffer[i] = value;
                }
                finally
                {
                    _readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        public void Dispose()
        {
            _readerWriterLockSlim?.Dispose();
        }


        public IEnumerator<T> GetEnumerator()
        {
            return EnumerateItems().ToList().GetEnumerator();
        }

        private IEnumerable<T> EnumerateItems()
        {
            _readerWriterLockSlim.EnterReadLock();
            try
            {
                var counterLength = _counter.Length;
                for (int i = 0; i < counterLength; i++)
                {
                    var physicalIndex = _counter.GetPhysicalIndex(i);
                    yield return _buffer[physicalIndex];
                }
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var enumerateItem in this.EnumerateItems())
            {
                stringBuilder.Append(enumerateItem);
                stringBuilder.Append(',');
            }

            return stringBuilder.ToString();
        }

        public void Add(IEnumerable<T> itemsEnumerable)
        {
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                foreach (var item in itemsEnumerable)
                {
                    _counter.Increment();
                    _buffer[_counter.Head] = item;
                }
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Add(IList<T> items)
        {
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                var addDifference = _counter.AddDifference((uint)(items.Count));
                var j = 0;
                foreach (var region in addDifference)
                {
                    for (int i = region.Tail; i <= region.Head; i++)
                    {
                        _buffer[i] = items[j];
                        j++;
                    }
                }
            }
            /*catch (Exception exception)
            {
                Debugger.Break();
            }*/
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Add(T item)
        {
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                var increment = _counter.Increment();
                _buffer[increment] = item;
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void ResetWith(IEnumerable<T> itemsEnumerable)
        {
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                _counter.Reset();
                foreach (var item in itemsEnumerable)
                {
                    _counter.Increment();
                    _buffer[_counter.Head] = item;
                }
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void ResetWith(IList<T> itemsEnumerable)
        {
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                _counter.Reset();
                var addDifference = _counter.AddDifference((uint)(itemsEnumerable.Count));
                var j = 0;
                foreach (var region in addDifference)
                {
                    for (int i = region.Tail; i <= region.Head; i++)
                    {
                        _buffer[i] = itemsEnumerable[j];
                        j++;
                    }
                }
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Clear()
        {
            this.Clear(true);
        }

        public void Clear(bool clearBuffer)
        {
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                _counter.Reset();
                if (clearBuffer)
                {
                    this._buffer.Clear();
                }
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            return Contains(item, default);
        }

        public bool Contains(T item, IEqualityComparer<T> comparer)
        {
            return this.IndexOf(item, comparer) > -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.EnumerateItems().ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return this.Remove(item, default);
        }

        public bool Remove(T item, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                for (int i = 0; i < _counter.Length; i++)
                {
                    var physicalIndex = _counter.GetPhysicalIndex(i);
                    if (comparer.Equals(_buffer[physicalIndex], item))
                    {
                        _buffer[physicalIndex] = default;
                    }
                }

                return false;
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }
    }
}