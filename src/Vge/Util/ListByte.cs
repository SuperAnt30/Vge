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
        private byte[] _buffer;
        /// <summary>
        /// Реальный размер массива
        /// </summary>
        private int _size;

        public ListByte(int size = 100)
        {
            _size = size;
            _buffer = new byte[size];
        }

        public byte this[int index]
        {
            get => _buffer[index];
            set => _buffer[index] = value;
        }

        /// <summary>
        /// Добавить значение
        /// </summary>
        public void Add(byte item)
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
        public void AddRange(byte[] items)
        {
            int count = items.Length;
            if (_size < Count + count)
            {
                _size = Count + count + (Count + count) / 2;
                Array.Resize(ref _buffer, _size);
            }
            Buffer.BlockCopy(items, 0, _buffer, Count, count);
            Count += count;
        }

        /// <summary>
        /// Добавить массив значений
        /// </summary>
        public void AddRange(ushort[] items)
        {
            int count = items.Length * 2;
            if (_size < Count + count)
            {
                _size = Count + count + (Count + count) / 2;
                Array.Resize(ref _buffer, _size);
            }
            Buffer.BlockCopy(items, 0, _buffer, Count, count);
            Count += count;
        }

        /// <summary>
        /// Сгенерировать массив
        /// </summary>
        public byte[] ToArray()
        {
            byte[] result = new byte[Count];
            Buffer.BlockCopy(_buffer, 0, result, 0, Count);
            return result;
        }

        /// <summary>
        /// Получить целый буфер
        /// </summary>
        public byte[] GetBufferAll() => _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Count = 0;
    }
}
