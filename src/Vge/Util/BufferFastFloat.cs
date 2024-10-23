using System;
using System.Runtime.InteropServices;

namespace Vge.Util
{
    /// <summary>
    /// Быстрый буферный массив для склейки рендера. тип Float
    /// Выделяем объём кеша, но увеличивать не может!!!
    /// </summary>
    public class BufferFastFloat : IDisposable
    {
        public int Count;
        public float[] Buffer;
        /// <summary>
        /// Получить адрес буфера
        /// </summary>
        private readonly IntPtr _hBuffer;
        /// <summary>
        /// Создаём, с выделенным объёмом
        /// </summary>
        public BufferFastFloat(int maxSize = 1000)
        {
            Buffer = new float[maxSize];
            _hBuffer = Marshal.AllocHGlobal(maxSize);
        }

        public float this[int index] => Buffer[index];

        public void Add(float item)
        {
            try
            {
                Buffer[Count++] = item;
            }
            catch
            {
                throw new Exception(Sr.GetString(Sr.OutOfRangeArray, Count));
            }
        }

        public void AddNull(int count) => Count += count;

        public void AddRange(float[] items)
        {
            try
            {
                int count = items.Length;
                for (int i = 0; i < count; i++)
                {
                    Buffer[Count + i] = items[i];
                }
                Count += count;
            }
            catch
            {
                throw new Exception(Sr.GetString(Sr.OutOfRangeArray, Count));
            }
        }

        public void AddRange(BufferFastFloat items)
        {
            try
            {
                int count = items.Count;
                for (int i = 0; i < count; i++)
                {
                    Buffer[Count + i] = items[i];
                }
                Count += count;
            }
            catch
            {
                throw new Exception(Sr.GetString(Sr.OutOfRangeArray, Count));
            }
        }

        public float[] ToArray()
        {
            float[] result = new float[Count];
            Array.Copy(Buffer, result, Count);
            return result;
        }

        public void Clear() => Count = 0;

        public void Combine(float[] items)
        {
            int count = items.Length;
            if (count > 0)
            {
                System.Buffer.BlockCopy(items, 0, Buffer, Count, count);
                Count += count;
            }
        }

        /// <summary>
        /// Получить адрес буфера
        /// </summary>
        public IntPtr ToBuffer()
        {
            Marshal.Copy(Buffer, 0, _hBuffer, Count * sizeof(float));
            return _hBuffer;
        }

        public void Dispose() => Marshal.FreeHGlobal(_hBuffer);
    }
}
