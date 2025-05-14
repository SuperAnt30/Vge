using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vge.Network
{
    /// <summary>
    /// Записывающий пакет данных в массив байт
    /// </summary>
    public class WritePacket
    {
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int _count;
        /// <summary>
        /// Массив
        /// </summary>
        private byte[] _buffer;
        /// <summary>
        /// Реальный размер массива
        /// </summary>
        private int _size;

        public WritePacket(int size = 64)
        {
            _size = size;
            _buffer = new byte[size];
        }

        public WritePacket(IPacket packet, int size = 64)
        {
            _size = size;
            _buffer = new byte[size];
            Byte(packet.Id);
            packet.WritePacket(this);
        }

        /// <summary>
        /// Внести загрузку пакета
        /// </summary>
        public void Trancive(IPacket packet)
        {
            _count = 0;
            Byte(packet.Id);
            packet.WritePacket(this);
        }

        /// <summary>
        /// Сгенерировать массив
        /// </summary>
        public byte[] ToArray()
        {
            byte[] result = new byte[_count];
            Buffer.BlockCopy(_buffer, 0, result, 0, _count);
            return result;
        }

        /// <summary>
        /// Добавить значение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(byte item)
        {
            if (_size <= _count)
            {
                _size = (int)(_size * 2f);
                Array.Resize(ref _buffer, _size);
            }
            _buffer[_count++] = item;
        }

        /// <summary>
        /// Добавить значение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(params byte[] items)
        {
            int c = items.Length;
            if (_size <= _count + c)
            {
                _size = (int)(_size + c + (_size * 0.5f));
                Array.Resize(ref _buffer, _size);
            }
            Buffer.BlockCopy(items, 0, _buffer, _count, c);

            _count += c;
        }

        /// <summary>
        /// Добавить часть массива
        /// </summary>
        private void AddRange(byte[] items, int index, int count)
        {
            if (_size <= _count + count)
            {
                _size = (int)(_size + count + (_size * 0.5f));
                Array.Resize(ref _buffer, _size);
            }
            Buffer.BlockCopy(items, index, _buffer, _count, count);

            _count += count;
        }

        #region Write

        /// <summary>
        /// Записать логический тип (0..1) 1 байт
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bool(bool value) => Add(value ? (byte)1 : (byte)0);

        /// <summary>
        /// Записать массив байт
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bytes(byte[] value)
        {
            Int(value.Length);
            if (value.Length > 0)
            {
                Add(value);
            }
        }
        /// <summary>
        /// Записать массив байт
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bytes(byte[] value, int index, int count)
        {
            if (value.Length >= count)
            {
                Int(count);
                AddRange(value, index, count);
            }
            else
            {
                Int(0);
            }
        }

        /// <summary>
        /// Записать массив байт c компрессией
        /// </summary>
        public void BytesCompress(byte[] value)
            => BytesCompress(value, 0, value.Length);
        /// <summary>
        /// Записать массив байт c компрессией
        /// </summary>
        public void BytesCompress(byte[] value, int index, int count)
        {
            if (value.Length > 0)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (GZipStream tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                    {
                        using (MemoryStream mStream = new MemoryStream(value, index, count))
                        {
                            mStream.CopyTo(tinyStream);
                        }
                    }
                    Bytes(outStream.ToArray());
                }
            }
            else
            {
                Int(0);
            }
        }

        /// <summary>
        /// Записать тип byte (0..255) 1 байт
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Byte(byte value) => Add(value);
        /// <summary>
        /// Записать тип ushort (0..65535) 2 байта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UShort(ushort value)
        {
            Add((byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF));
        }
        /// <summary>
        /// Записать тип uint (0..4 294 967 295) 4 байта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UInt(uint value)
        {
            Add((byte)((value & 0xFF000000) >> 24),
                (byte)((value & 0xFF0000) >> 16),
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF));
        }

        /// <summary>
        /// Записать тип ulong (0..18 446 744 073 709 551 615) 8 байт
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ULong(ulong value)
        {
            Add((byte)((value & 0xFF00000000000000) >> 56),
                (byte)((value & 0xFF000000000000) >> 48),
                (byte)((value & 0xFF0000000000) >> 40),
                (byte)((value & 0xFF00000000) >> 32),
                (byte)((value & 0xFF000000) >> 24),
                (byte)((value & 0xFF0000) >> 16),
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF));
        }

        /// <summary>
        /// Записать тип sbyte (-128..127) 1 байт
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SByte(sbyte value) => Byte((byte)value);
        /// <summary>
        /// Записать тип short (-32768..32767) 2 байта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Short(short value) => UShort((ushort)value);
        /// <summary>
        /// Записать тип int (-2 147 483 648..2 147 483 647) 4 байта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Int(int value) => UInt((uint)value);
        /// <summary>
        /// Записать тип long (–9 223 372 036 854 775 808..9 223 372 036 854 775 807) 8 байт
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Long(long value) => ULong((ulong)value);

        /// <summary>
        /// Записать строку в UTF-16
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void String(string value)
            => Bytes(Encoding.BigEndianUnicode.GetBytes(value));

        /// <summary>
        /// Записать тип float (точность 0,0001) 4 байта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Float(float value) 
            //=> Int((int)(value * 10000)); // Этот быстрее на ~10-20%
            => Add(BitConverter.GetBytes(value));

        #endregion
    }
}
