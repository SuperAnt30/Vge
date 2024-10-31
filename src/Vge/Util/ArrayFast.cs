using System;

namespace Vge.Util
{
    /// <summary>
    /// Быстрый массив, без проверки длинны
    /// </summary>
    public class ArrayFast<T>
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

        public ArrayFast(int size = 100)
        {
            _size = size;
            _buffer = new T[size];
        }

        public T this[int index] => _buffer[index];


        /// <summary>
        /// Изменить размер массива
        /// </summary>
        public void Resize(int newSize)
        {
            _size = newSize;
            Array.Resize(ref _buffer, _size);
            if (Count >= _size) Count = _size;
        }

        /// <summary>
        /// Добавить значение
        /// </summary>
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
        /// Очистить
        /// </summary>
        public void Clear() => Count = 0;

        public override string ToString() => Count.ToString();
    }
}
