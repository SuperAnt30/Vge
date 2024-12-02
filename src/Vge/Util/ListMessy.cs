using System;
using System.Runtime.CompilerServices;

namespace Vge.Util
{
    /// <summary>
    /// Беспорядочный список, не придерживается очерёдности при удалении
    /// </summary>
    public class ListMessy<T>
    {
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Массив
        /// </summary>
        private T[] _buffer;
        /// <summary>
        /// Реальный размер массива
        /// </summary>
        private int _size;

        public ListMessy(int size = 128)
        {
            _size = size;
            _buffer = new T[size];
        }

        public T this[int index] => _buffer[index];

        /// <summary>
        /// Добавить значение без проверки размера
        /// </summary>
        public void AddNotCheckSize(T item) => _buffer[Count++] = item;

        /// <summary>
        /// Добавить значение
        /// </summary>
        public void Add(T item)
        {
            if (_size < Count + 1)
            {
                _size = (int)(_size * 1.5f);
                Array.Resize(ref _buffer, _size);
            }
            _buffer[Count++] = item;
        }

        /// <summary>
        /// Добавить массив
        /// </summary>
        public void AddRange(T[] items)
        {
            int count = items.Length;
            if (_size < Count + count)
            {
                _size = (int)(_size + count + (_size * 0.3f));
                Array.Resize(ref _buffer, _size);
            }
            Array.Copy(items, _buffer, Count);
            Count += count;
        }

        /// <summary>
        /// Найти имеется ли такое значение
        /// </summary>
        public bool Contains(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_buffer[i].Equals(item)) return true;
            }
            return false;
        }

        /// <summary>
        /// Сделать копию в массив
        /// </summary>
        public T[] ToArray()
        {
            T[] result = new T[Count];
            Array.Copy(_buffer, result, Count);
            return result;
        }

        /// <summary>
        /// Удалить значение если имеется
        /// </summary>
        public bool Remove(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_buffer[i].Equals(item))
                {
                    int last = Count - 1;
                    if (last != i)
                    {
                        _buffer[i] = _buffer[last];
                    }
                    Count--;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Удалить значение по индексу
        /// </summary>
        public bool RemoveAt(int index)
        {
            if (index < Count)
            {
                int last = Count - 1;
                if (last != index)
                {
                    _buffer[index] = _buffer[last];
                }
                Count--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Удалить последнее значение
        /// </summary>
        public void RemoveLast()
        {
            if (Count > 0) Count--;
        }

        /// <summary>
        /// Очистить
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Count = 0;

        public override string ToString() => Count.ToString();
    }
}
