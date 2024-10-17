using System;
using System.IO;
using System.IO.Compression;
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
        private int count;
        /// <summary>
        /// Массив
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// Реальный размер массива
        /// </summary>
        private int size;

        private WritePacket(int size = 64)
        {
            this.size = size;
            buffer = new byte[size];
        }

        public static byte[] TranciveToArray(IPacket packet)
        {
            WritePacket writePacket = new WritePacket();
            writePacket.Trancive(packet);
            return writePacket.ToArray();
        }

        /// <summary>
        /// Внести загрузку пакета
        /// </summary>
        private void Trancive(IPacket packet)
        {
            count = 0;
            Byte(packet.Id);
            packet.WritePacket(this);
        }

        /// <summary>
        /// Сгенерировать массив
        /// </summary>
        private byte[] ToArray()
        {
            byte[] result = new byte[count];
            Array.Copy(buffer, result, count);
            return result;
        }

        /// <summary>
        /// Добавить значение
        /// </summary>
        private void Add(byte item)
        {
            if (size <= count)
            {
                size = (int)(size * 2f);
                Array.Resize(ref buffer, size);
            }
            buffer[count++] = item;
        }

        /// <summary>
        /// Добавить значение
        /// </summary>
        private void Add(params byte[] items)
        {
            int c = items.Length;
            if (size <= count + c)
            {
                size = (int)(size + c + (size * 0.5f));
                Array.Resize(ref buffer, size);
            }
            Buffer.BlockCopy(items, 0, buffer, count, c);

            count += c;
        }

        #region Write

        /// <summary>
        /// Записать логический тип (0..1) 1 байт
        /// </summary>
        public void Bool(bool value) => Add(value ? (byte)1 : (byte)0);

        /// <summary>
        /// Записать массив байт
        /// </summary>
        public void Bytes(byte[] value)
        {
            UShort((ushort)value.Length);
            if (value.Length > 0)
            {
                Add(value);
            }
        }

        /// <summary>
        /// Записать массив байт c компрессией
        /// </summary>
        public void BytesCompress(byte[] value)
        {
            if (value.Length > 0)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (GZipStream tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                    using (MemoryStream mStream = new MemoryStream(value))
                        mStream.CopyTo(tinyStream);
                    Bytes(outStream.ToArray());
                }
            }
            else
            {
                UShort(0);
            }
        }

        /// <summary>
        /// Записать тип byte (0..255) 1 байт
        /// </summary>
        public void Byte(byte value) => Add(value);
        /// <summary>
        /// Записать тип ushort (0..65535) 2 байта
        /// </summary>
        public void UShort(ushort value)
        {
            Add((byte)((value & 0xFF00) >> 8), 
                (byte)(value & 0xFF));
        }
        /// <summary>
        /// Записать тип uint (0..4 294 967 295) 4 байта
        /// </summary>
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
        public void SByte(sbyte value) => Byte((byte)value);
        /// <summary>
        /// Записать тип short (-32768..32767) 2 байта
        /// </summary>
        public void Short(short value) => UShort((ushort)value);
        /// <summary>
        /// Записать тип int (-2 147 483 648..2 147 483 647) 4 байта
        /// </summary>
        public void Int(int value) => UInt((uint)value);
        /// <summary>
        /// Записать тип long (–9 223 372 036 854 775 808..9 223 372 036 854 775 807) 8 байт
        /// </summary>
        public void Long(long value) => ULong((ulong)value);

        /// <summary>
        /// Записать строку в UTF-16
        /// </summary>
        public void String(string value)
            => Bytes(Encoding.BigEndianUnicode.GetBytes(value));

        /// <summary>
        /// Записать тип float (точность 0,0001) 4 байта
        /// </summary>
        public void Float(float value) 
            => Int((int)(value * 10000)); // Этот быстрее на ~10-20%
         // => Write(BitConverter.GetBytes(value), 0, 4);

        #endregion
    }
}
