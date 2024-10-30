using System;
using System.Runtime.InteropServices;

namespace Vge.Renderer.World
{
    public interface IBufferFast
    {
        /// <summary>
        /// Копировать в буффер
        /// </summary>
        void CopyBuffer(IntPtr intPtr);

        /// <summary>
        /// Размер буфера
        /// </summary>
        int ToSize();
    }

    /// <summary>
    /// Быстрый буферный массив для склейки рендера.
    /// Выделяем объём кеша, но увеличивать не может!!!
    /// </summary>
    public abstract class BufferFast<T> : IBufferFast, IDisposable
    {
        /// <summary>
        /// Количество используемых элементов
        /// </summary>
        public int Count;
        /// <summary>
        /// Массив
        /// </summary>
        protected T[] _buffer;
        /// <summary>
        /// Получить адрес буфера
        /// </summary>
        protected IntPtr _hBuffer;

        public BufferFast(int maxSize = 1000)
            => _buffer = new T[maxSize];

        public T this[int index] => _buffer[index];

        public void Add(T item)
        {
            try
            {
                _buffer[Count++] = item;
            }
            catch
            {
                throw new Exception(Sr.GetString(Sr.OutOfRangeArray, Count));
            }
        }

        /// <summary>
        /// Добавить несколько пустых или сулчайных значения
        /// </summary>
        public void AddNull(int count) => Count += count;

        /// <summary>
        /// Добавить одно пустые или сулчайные значения
        /// </summary>
        public void AddNull() => Count++;

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear() => Count = 0;

        public T[] ToArray()
        {
            T[] result = new T[Count];
            Array.Copy(_buffer, result, Count);
            return result;
        }

        /// <summary>
        /// Получить адрес буфера
        /// </summary>
        public virtual IntPtr ToBuffer() => _hBuffer;

        /// <summary>
        /// Копировать в буффер
        /// </summary>
        public virtual void CopyBuffer(IntPtr intPtr) { }

        /// <summary>
        /// Размер буфера
        /// </summary>
        public virtual int ToSize() => Count;

        public virtual void Dispose() => Marshal.FreeHGlobal(_hBuffer);
    }
}
