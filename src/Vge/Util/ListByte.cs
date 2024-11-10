using System;
using System.Runtime.CompilerServices;

namespace Vge.Util
{
    /// <summary>
    /// Усовершенствованный лист типа flout, работы без мусора, чтоб не пересоздавать
    /// </summary>
    public class ListByte
    {
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Массив
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// Реальный размер массива
        /// </summary>
        private int size;

        public ListByte(int size = 100)
        {
            this.size = size;
            buffer = new byte[size];
        }

        public byte this[int index]
        {
            get => buffer[index];
            set => buffer[index] = value;
        }

        /// <summary>
        /// Добавить значение
        /// </summary>
        public void Add(byte item)
        {
            if (size <= Count)
            {
                size = (int)(size * 1.5f);
                Array.Resize(ref buffer, size);
            }
            buffer[Count++] = item;
        }

        /// <summary>
        /// Добавить массив значений
        /// </summary>
        public void AddRange(byte[] items)
        {
            int count = items.Length;
            if (size < Count + count)
            {
                size = Count + count + (Count + count) / 2;
                Array.Resize(ref buffer, size);
            }
            Buffer.BlockCopy(items, 0, buffer, Count, count);
            Count += count;
        }

        /// <summary>
        /// Добавить массив значений
        /// </summary>
        public void AddRange(ushort[] items)
        {
            int count = items.Length * 2;
            if (size < Count + count)
            {
                size = Count + count + (Count + count) / 2;
                Array.Resize(ref buffer, size);
            }
            Buffer.BlockCopy(items, 0, buffer, Count, count);
            Count += count;
        }

        /// <summary>
        /// Сгенерировать массив
        /// </summary>
        public byte[] ToArray()
        {
            byte[] result = new byte[Count];
            Buffer.BlockCopy(buffer, 0, result, 0, Count);
            return result;
        }

        /// <summary>
        /// Получить целый буфер
        /// </summary>
        public byte[] GetBufferAll() => buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Count = 0;
    }
}
