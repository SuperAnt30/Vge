using System;
using System.Runtime.CompilerServices;

namespace Vge.Util
{
    /// <summary>
    /// Усовершенствованный лист типа flout, работы без мусора, чтоб не пересоздавать
    /// </summary>
    public class ListFlout
    {
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Массив
        /// </summary>
        private float[] buffer;
        /// <summary>
        /// Реальный размер массива
        /// </summary>
        private int size;
        /// <summary>
        /// Размер float в байтах
        /// </summary>
        private readonly static byte sizeType = sizeof(float);

        public ListFlout(int size = 100)
        {
            this.size = size;
            buffer = new float[size];
        }

        public float this[int index]
        {
            get => buffer[index];
            set => buffer[index] = value;
        }

        /// <summary>
        /// Добавить значение
        /// </summary>
        public void Add(float item)
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
        /// <param name="sizeType">Размер типа в байтах</param>
        public void AddRange(float[] items)
        {
            int count = items.Length;
            if (size < Count + count)
            {
                size = Count + count + (Count + count) / 2;
                Array.Resize(ref buffer, size);
            }
            Buffer.BlockCopy(items, 0, buffer, Count * sizeType, count * sizeType);
            Count += count;
        }

        /// <summary>
        /// Добавить копию из текущего буффера в конец
        /// </summary>
        /// <param name="offset">откуда начинаем буфер</param>
        /// <param name="count">количество элементов буфера</param>
        public void AddCopy(int offset, int count)
        {
            if (size < Count + count)
            {
                size += Count + count + (Count + count) / 2;
                Array.Resize(ref buffer, size);
            }
            Buffer.BlockCopy(buffer, offset, buffer, Count * sizeType, count * sizeType);
            Count += count;
        }

        /// <summary>
        /// Добавить копию из текущего буффера в определённое место dstOffset
        /// </summary>
        /// <param name="srcOffset">откуда начинаем буфер</param>
        /// <param name="count">количество элементов буфера</param>
        /// /// <param name="dstOffset">откуда начинаем заливать буфер</param>
        public void AddCopy(int srcOffset, int count, int dstOffset)
        {
            if (size < dstOffset + count)
            {
                size += dstOffset + count + (dstOffset + count) / 2;
                Array.Resize(ref buffer, size);
            }
            Buffer.BlockCopy(buffer, srcOffset, buffer, dstOffset * sizeType, count * sizeType);
            if (dstOffset + count > Count)
            {
                Count = dstOffset + count;
            }
        }

        /// <summary>
        /// Найти имеется ли такое значение
        /// </summary>
        public bool Contains(float item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (buffer[i].Equals(item)) return true;
            }
            return false;
        }

        /// <summary>
        /// Сгенерировать массив
        /// </summary>
        public float[] ToArray()
        {
            float[] result = new float[Count];
            Array.Copy(buffer, result, Count);
            return result;
        }

        /// <summary>
        /// Получить целый буфер
        /// </summary>
        public float[] GetBufferAll() => buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Count = 0;
    }
}
