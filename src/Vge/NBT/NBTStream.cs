using System;
using System.IO;
using System.Text;

namespace Vge.NBT
{
    /// <summary>
    /// Стрим для NBT
    /// </summary>
    public class NBTStream : MemoryStream
    {
        #region Read

        /// <summary>
        /// Прочесть текстовую строку utf8
        /// </summary>
        public string ReadUTF()
        {
            Encoding stringEncoding = Encoding.UTF8;
            short length = ReadShort();
            if (length == 0) return string.Empty;
            // попробовать! было ранее другая методика ReadArray
            byte[] data = new byte[length];
            Read(data, 0, length);
            return stringEncoding.GetString(data);
        }

        /// <summary>
        /// Прочесть тип byte (0..255) 1 байт
        /// </summary>
        public byte Byte()
        {
            int value = ReadByte();
            if (value == -1) throw new EndOfStreamException();
            return (byte)value;
        }

        public short ReadShort() => (short)((Byte() << 8) | Byte());
        public int ReadInt() => (Byte() << 24) | (Byte() << 16) | (Byte() << 8) | Byte();
        public long ReadLong() => ((long)(Byte() << 8) | Byte()) << 48 | ((long)(Byte() << 8) | Byte()) << 32
                | ((long)(Byte() << 8) | Byte()) << 16 | ((long)(Byte() << 8) | Byte());

        /// <summary>
        /// Прочесть число с плавоющей запятой
        /// </summary>
        public float ReadFloat()
        {
            byte[] buffer = new byte[4];
            buffer[3] = Byte();
            buffer[2] = Byte();
            buffer[1] = Byte();
            buffer[0] = Byte();
            return BitConverter.ToSingle(buffer, 0);
        }

        #endregion

        #region Write

        /// <summary>
        /// Записать текстовую строку utf8
        /// </summary>
        public void WriteUTF(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            WriteShort((short)buffer.Length);
            Write(buffer, 0, buffer.Length);
        }

        public void WriteShort(short number)
        {
            ushort value = (ushort)number;
            Write(new[] {
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF)
            }, 0, 2);
        }

        public void WriteInt(int number)
        {
            uint value = (uint)number;
            Write(new[] {
                (byte)((value & 0xFF000000) >> 24),
                (byte)((value & 0xFF0000) >> 16),
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF)
            }, 0, 4);
        }

        public void WriteLong(long number)
        {
            ulong value = (ulong)number;
            Write(new[] {
                (byte)((value & 0xFF00000000000000) >> 56),
                (byte)((value & 0xFF000000000000) >> 48),
                (byte)((value & 0xFF0000000000) >> 40),
                (byte)((value & 0xFF00000000) >> 32),
                (byte)((value & 0xFF000000) >> 24),
                (byte)((value & 0xFF0000) >> 16),
                (byte)((value & 0xFF00) >> 8),
                (byte)(value & 0xFF)
            }, 0, 8);
        }

        /// <summary>
        /// Сохранить число с плавоющей запятой
        /// </summary>
        public void WriteFloat(float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Write(new[] { buffer[3], buffer[2], buffer[1], buffer[0] }, 0, 4);
        }

        #endregion
    }
}
