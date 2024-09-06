using System;
using System.Runtime.CompilerServices;

namespace Vge.Util
{
    /// <summary>
    /// Усовершенствованный лист от Мефистофель, работы без мусора, чтоб не пересоздавать
    /// </summary>
    public class ListFast<T>
    {
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Массив
        /// </summary>
        private T[] buffer;
        /// <summary>
        /// Реальный размер массива
        /// </summary>
        private int size;

        public ListFast(int size = 100)
        {
            this.size = size;
            buffer = new T[size];
        }

        public T this[int index] => buffer[index];

        /// <summary>
        /// Добавить значение
        /// </summary>
        public void Add(T item)
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
        public void AddRange(T[] items)
        {
            int count = items.Length;
            if (size < Count + count)
            {
                size = (int)(size + count + (size * 0.3f));
                Array.Resize(ref buffer, size);
            }
            for (int i = 0; i < count; i++)
                buffer[Count + i] = items[i];

            Count += count;
        }

        /// <summary>
        /// Найти имеется ли такое значение
        /// </summary>
        public bool Contains(T item)
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
        public T[] ToArray()
        {
            T[] result = new T[Count];
            Array.Copy(buffer, result, Count);
            return result;
        }

        /// <summary>
        /// Получить целый буфер
        /// </summary>
        public T[] GetBufferAll() => buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Count = 0;
    }
}
