using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GLChart.Core.Collection
{
    /// <summary>
    /// 内存对齐的浮点数组
    /// </summary>
    public class AlignedFloatArray : IList<float>, IDisposable
    {
        public const int AVXAlignment = 32;

        public const int SSEAlignment = 16;

        private byte[] _buffer;
        private GCHandle _bufferHandle;

        private IntPtr _bufferPointer;
        private readonly int _length;

        public AlignedFloatArray(int length, int byteAlignment = AVXAlignment)
        {
            this._length = length;
            _buffer = new byte[length * sizeof(float) + byteAlignment];
            _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            long ptr = _bufferHandle.AddrOfPinnedObject().ToInt64();
            // round up ptr to nearest 'byteAlignment' boundary
            ptr = (ptr + byteAlignment - 1) & ~(byteAlignment - 1);
            _bufferPointer = new IntPtr(ptr);
        }

        private bool _isDisposed;

        ~AlignedFloatArray()
        {
            if (!_isDisposed)
            {
                Dispose(false);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Dispose(bool disposing)
        {
            if (_bufferHandle.IsAllocated)
            {
                _bufferHandle.Free();
                _buffer = null;
            }

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        public int IndexOf(float item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, float item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public float this[int index]
        {
            get
            {
                unsafe
                {
                    return GetPointer()[index];
                }
            }
            set
            {
                unsafe
                {
                    GetPointer()[index] = value;
                }
            }
        }

        public int Length
        {
            get { return _length; }
        }

        public void Write(int index, float[] src, int srcIndex, int count)
        {
            if (index < 0 || index >= _length) throw new IndexOutOfRangeException();

            if ((index + count) > _length) count = Math.Max(0, _length - index);

            Marshal.Copy(
                src,
                srcIndex,
                new IntPtr(_bufferPointer.ToInt64() + index * sizeof(float)),
                count);
        }

        public void Read(int index, float[] dest, int dstIndex, int count)
        {
            if (index < 0 || index >= _length) throw new IndexOutOfRangeException();
            if ((index + count) > _length) count = Math.Max(0, _length - index);
            Marshal.Copy(
                new IntPtr(_bufferPointer.ToInt64() + index * sizeof(float)),
                dest,
                dstIndex,
                count);
        }

        public float[] GetManagedArray()
        {
            return GetManagedArray(0, _length);
        }

        public float[] GetManagedArray(int index, int count)
        {
            float[] result = new float[count];

            Read(index, result, 0, count);
            return result;
        }

        public IEnumerator<float> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');

            for (int t = 0; t < _length; t++)
            {
                sb.Append(this[t].ToString());

                if (t < (_length - 1)) sb.Append(',');
            }

            sb.Append(']');
            return sb.ToString();
        }

        public unsafe float* GetPointer(int index)
        {
            return GetPointer() + index;
        }

        public unsafe float* GetPointer()
        {
            return ((float*) _bufferPointer.ToPointer());
        }

        public void Add(float item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(float item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(float[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(float item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
    }
}