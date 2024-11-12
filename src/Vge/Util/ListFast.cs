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
        private T[] _buffer;
        /// <summary>
        /// Реальный размер массива
        /// </summary>
        private int _size;

        public ListFast(int size = 100)
        {
            _size = size;
            _buffer = new T[size];
        }

        public T this[int index] => _buffer[index];

        /// <summary>
        /// Добавить значение
        /// </summary>
        public void Add(T item)
        {
            if (_size <= Count)
            {
                _size = (int)(_size * 1.5f);
                Array.Resize(ref _buffer, _size);
            }
            _buffer[Count++] = item;
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
        /// Получить целый буфер
        /// </summary>
        public T[] GetBufferAll() => _buffer;

        /// <summary>
        /// Удалить последнее значение
        /// </summary>
        public void RemoveLast()
        {
            if (Count > 0) Count--;
        }

        /// <summary>
        /// Вернуть последнее значение
        /// </summary>
        public T GetLast() => _buffer[Count - 1];

        /// <summary>
        /// Присвоить значение null
        /// </summary>
        public void IndexNull(int index) => _buffer[index] = default(T);

        public void Sort()
            => Array.Sort<T>(_buffer, 0, Count, null);

        /// <summary>
        /// Очистить
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Count = 0;

        public override string ToString() => Count.ToString();
    }
}
