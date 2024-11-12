using System;
using System.Runtime.InteropServices;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Быстрый буферный массив для склейки рендера. тип Float
    /// Выделяем объём кеша, но увеличивать не может!!!
    /// </summary>
    public class BufferFastFloat : BufferFast<float>
    {
        /// <summary>
        /// Создаём, с выделенным объёмом
        /// </summary>
        public BufferFastFloat(int maxSize = 1000) : base(maxSize)
            => _hBuffer = Marshal.AllocHGlobal(maxSize * sizeof(float));

        /// <summary>
        /// Добавить массив значений
        /// </summary>
        public void AddRange(float[] items)
        {
            try
            {
                int count = items.Length;
                Buffer.BlockCopy(items, 0, _buffer, Count * sizeof(float), count * sizeof(float));
                Count += count;
            }
            catch
            {
                throw new Exception(Sr.GetString(Sr.OutOfRangeArray, Count));
            }
        }

        /// <summary>
        /// Получить адрес буфера
        /// </summary>
        public override IntPtr ToBuffer()
        {
            Marshal.Copy(_buffer, 0, _hBuffer, Count * sizeof(float));
            return _hBuffer;
        }

        /// <summary>
        /// Копировать в буффер
        /// </summary>
        public override void CopyBuffer(IntPtr intPtr)
            => Marshal.Copy(_buffer, 0, intPtr, Count);

        /// <summary>
        /// Размер буфера
        /// </summary>
        public override int ToSize() => Count * sizeof(float);

        public override void Dispose()
        {
            base.Dispose();
            _buffer = null;
        }
    }
}
