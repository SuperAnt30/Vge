using System;
using System.Runtime.InteropServices;

namespace Vge.Util
{
    /// <summary>
    /// Быстрый буферный массив для склейки рендера.
    /// Выделяем объём кеша, но увеличивать не может!!!
    /// </summary>
    public class BufferFast : IDisposable
    {
        public int Count;
        public byte[] Buffer;
        /// <summary>
        /// Получить адрес буфера
        /// </summary>
        private readonly IntPtr _hBuffer;
        /// <summary>
        /// Создаём, с выделенным объёмом
        /// </summary>
        public BufferFast(int maxSize = 1000)
        {
            Buffer = new byte[maxSize];
            _hBuffer = Marshal.AllocHGlobal(maxSize);
        }

        public byte this[int index] => Buffer[index];

        public void Add(byte item)
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

        public void AddRange(byte[] items)
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

        public void AddRange(BufferFast items)
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

        public void AddFloat(byte[] items)
        {
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    Buffer[Count + i] = items[i];
                }
                Count += 4;
            }
            catch
            {
                throw new Exception(Sr.GetString(Sr.OutOfRangeArray, Count));
            }
        }

        public byte[] ToArray()
        {
            byte[] result = new byte[Count];
            Array.Copy(Buffer, result, Count);
            return result;
        }

        public void Clear() => Count = 0;

        public void Combine(byte[] items)
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
            Marshal.Copy(Buffer, 0, _hBuffer, Count);
            return _hBuffer;
        }

        public void Dispose() => Marshal.FreeHGlobal(_hBuffer);
    }
}
