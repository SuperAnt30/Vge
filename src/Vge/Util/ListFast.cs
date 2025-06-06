using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Vge.Util
{
    /// <summary>
    /// Усовершенствованный лист от Мефистофель, работы без мусора, чтоб не пересоздавать
    /// </summary>
    public class ListFast<T> : IEnumerable
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
        /// Добавить массив значений
        /// </summary>
        public void AddRange(T[] items)
        {
            int count = items.Length;
            if (_size < Count + count)
            {
                _size = Count + count + (Count + count) / 2;
                Array.Resize(ref _buffer, _size);
            }
            Array.Copy(items, 0, _buffer, Count, count);
            //Buffer.BlockCopy(items, 0, _buffer, Count * _sizeType, count * _sizeType);
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

        /// <summary>
        /// Очистить полностью, с удалением данных
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFull()
        {
            if (Count > 0)
            {
                Array.Clear(_buffer, 0, Count);
                Count = 0;
            }
        }

        public override string ToString() => Count.ToString();

        /// <summary>
        /// Благодаря такой реализации мы можем перебирать объекты в цикле foreach
        /// </summary>
        public IEnumerator GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator
        {
            private readonly ListFast<T> _list;
            private int _index;
            private T _current;

            /// <summary>
            /// Возвращает элемент, расположенный в текущей позиции перечислителя
            /// </summary>
            public T Current { get { return _current; } }
            /// <summary>
            /// Возвращает элемент, расположенный в текущей позиции перечислителя
            /// </summary>
            object IEnumerator.Current { get { return Current; } }

            internal Enumerator(ListFast<T> list)
            {
                _list = list;
                _index = 0;
                _current = default(T);
            }

            /// <summary>
            /// Перемещает перечислитель к следующему элементу коллекции ListFast
            /// </summary>
            public bool MoveNext()
            {
                if (_index < _list.Count)
                {
                    _current = _list._buffer[_index];
                    _index++;
                    return true;
                }
                _index = _list.Count + 1;
                _current = default(T);
                return false;
            }

            /// <summary>
            /// Устанавливает перечислитель в его начальное положение, т. е. перед первым элементом коллекции
            /// </summary>
            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default(T);
            }
        }
    }
}
