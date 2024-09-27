using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Vge.Network
{
    /// <summary>
    /// Объект который из буфера данных склеивает пакеты
    /// </summary>
    public struct ReadPacket
    {
        /// <summary>
        /// Массив
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// Расположение где читаем
        /// </summary>
        private int position;

        public IPacket Receive(byte[] buffer, IPacket packet)
        {
            this.buffer = buffer;
            position = 1;
            packet.ReadPacket(this);
            return packet;
        }

        #region Read

        /// <summary>
        /// Прочесть логический тип (0..1) 1 байт
        /// </summary>
        public bool Bool() => buffer[position++] != 0;

        /// <summary>
        /// Прочесть массив байт
        /// </summary>
        public byte[] Bytes()
        {
            // Первый параметр длинна массива
            ushort count = UShort();
            byte[] b = new byte[count];
            Buffer.BlockCopy(buffer, position, b, 0, count);
            position += count;
            return b;
        }

        /// <summary>
        /// Прочесть массив байт c декомпрессией
        /// </summary>
        public byte[] BytesDecompress()
        {
            using (MemoryStream inStream = new MemoryStream(Bytes()))
            using (GZipStream bigStream = new GZipStream(inStream, CompressionMode.Decompress))
            using (MemoryStream bigStreamOut = new MemoryStream())
            {
                bigStream.CopyTo(bigStreamOut);
                return bigStreamOut.ToArray();
            }
        }

        /// <summary>
        /// Прочесть тип byte (0..255) 1 байт
        /// </summary>
        public byte Byte() => buffer[position++];
        /// <summary>
        /// Прочесть тип ushort (0..65535) 2 байта
        /// </summary>
        public ushort UShort() => (ushort)((buffer[position++] << 8) | buffer[position++]);
        /// <summary>
        /// Прочесть тип uint (0..4 294 967 295) 4 байта
        /// </summary>
        public uint UInt() => (uint)((buffer[position++] << 24) | (buffer[position++] << 16) 
            | (buffer[position++] << 8) | buffer[position++]);
        /// <summary>
        /// Прочесть тип uint (0..18 446 744 073 709 551 615) 8 байт
        /// </summary>
        public ulong ULong() => (ulong)((buffer[position++] << 56) | (buffer[position++] << 48) 
            | (buffer[position++] << 40) | (buffer[position++] << 32)
            | (buffer[position++] << 24) | (buffer[position++] << 16) 
            | (buffer[position++] << 8) | buffer[position++]);
        /// <summary>
        /// Прочесть тип sbyte (-128..127) 1 байт
        /// </summary>
        public sbyte SByte() => (sbyte)Byte();
        /// <summary>
        /// Прочесть тип short (-32768..32767) 2 байта
        /// </summary>
        public short ReadShort() => (short)UShort();
        /// <summary>
        /// Прочесть тип int (-2 147 483 648..2 147 483 647) 4 байта
        /// </summary>
        public int Int() => (int)UInt();
        /// <summary>
        /// Прочесть тип int (–9 223 372 036 854 775 808..9 223 372 036 854 775 807) 8 байт
        /// </summary>
        public long Long() => (long)ULong();

        /// <summary>
        /// Прочесть строку в UTF-16
        /// </summary>
        public string String() => Encoding.BigEndianUnicode.GetString(Bytes());

        /// <summary>
        /// Прочесть тип float (точность 0,0001) 4 байта
        /// </summary>
        public float Float() => Int() / 10000f; // Этот быстрее на ~10-20%
        //public float Float() => BitConverter.ToSingle(new byte[] { Byte(), Byte(), Byte(), Byte() }, 0);

        #endregion

        
    }
}
