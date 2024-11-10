using System;
using System.Collections.Generic;
using Vge.World.Chunk;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Отправляем клиенту изменённые псевдо чанки
    /// </summary>
    public struct PacketS21ChunkData : IPacket, IDisposable
    {
        public byte Id => 0x21;

        // TDOO::2024-11-10 ЗАМЕНИТЬ на List
        /// <summary>
        /// Статический быстрый буфер для записи
        /// </summary>
        //private static readonly ListByte _bufferWrite = new ListByte(50000);

        /// <summary>
        /// Буффер записи чанка, статический.
        /// Размер надо определить от максималки высот чанков и данных
        /// </summary>
        private static byte[] _bufferWrite = new byte[285000];
        /// <summary>
        /// Количество элементов используемых для буфера записи
        /// </summary>
        private static int _count;

        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public int CurrentChunkX { get; private set; }
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public int CurrentChunkY { get; private set; }
        /// <summary>
        /// Буффер чтения чанка
        /// </summary>
        public byte[] BufferRead { get; private set; }
        /// <summary>
        /// Флаг псевдо чанков
        /// </summary>
        public int FlagsYAreas { get; private set; }
        /// <summary>
        /// Данные столбца биома, как правило при первой загрузке
        /// </summary>
        public bool IsBiom { get; private set; }

        /// <summary>
        /// Выгрузить чанк у игрока
        /// </summary>
        public PacketS21ChunkData(int chunkPosX, int chunkPosY)
        {
            CurrentChunkX = chunkPosX;
            CurrentChunkY = chunkPosY;
            FlagsYAreas = 0;
            BufferRead = null;
            IsBiom = false;
        }
        /// <summary>
        /// Загрузка данных чанка
        /// </summary>
        public PacketS21ChunkData(ChunkBase chunk, bool biom, int flagsYAreas)
        {
            if (flagsYAreas == 0)
            {
                throw new Exception(Sr.GetString(Sr.OutOfRange, flagsYAreas));
            }

            CurrentChunkX = chunk.CurrentChunkX;
            CurrentChunkY = chunk.CurrentChunkY;
            FlagsYAreas = flagsYAreas;
            IsBiom = biom;
            BufferRead = null;
            _count = _ChunkBufferWrite(chunk, biom, flagsYAreas, _bufferWrite);
        }

        /// <summary>
        /// Записать данные чанка в буфер
        /// </summary>
        private int _ChunkBufferWrite(ChunkBase chunk, bool biom, int flagsYAreas, byte[] buf)
        {
            ushort data;
            int i, y;
            uint value;
            ChunkStorage chunkStorage;

            int count = 0;
            for (y = 0; y < chunk.NumberSections; y++)
            {
                if ((flagsYAreas & 1 << y) != 0)
                {
                    chunkStorage = chunk.StorageArrays[y];

                    Buffer.BlockCopy(chunkStorage.LightBlock, 0, buf, count, chunkStorage.LightBlock.Length);
                    count += chunkStorage.LightBlock.Length;
                    Buffer.BlockCopy(chunkStorage.LightSky, 0, buf, count, chunkStorage.LightSky.Length);
                    count += chunkStorage.LightSky.Length;

                    if (chunkStorage.IsEmptyData())
                    {
                        buf[count++] = 0;
                    }
                    else
                    {
                        buf[count++] = 1;
                        Buffer.BlockCopy(chunkStorage.Data, 0, buf, count, chunkStorage.Data.Length * 2);
                        count += chunkStorage.Data.Length * 2;

                        data = (ushort)chunkStorage.Metadata.Count;
                        buf[count++] = (byte)(data >> 8);
                        buf[count++] = (byte)(data & 0xFF);

                        foreach (KeyValuePair<ushort, uint> entry in chunkStorage.Metadata)
                        {
                            buf[count++] = (byte)(entry.Key >> 8);
                            buf[count++] = (byte)(entry.Key & 0xFF);
                            value = entry.Value;
                            buf[count++] = (byte)((value & 0xFF000000) >> 24);
                            buf[count++] = (byte)((value & 0xFF0000) >> 16);
                            buf[count++] = (byte)((value & 0xFF00) >> 8);
                            buf[count++] = (byte)(value & 0xFF);
                        }
                    }
                }
            }
            if (biom)
            {
                // добавляем данные биома
                for (i = 0; i < 256; i++)
                {
                    buf[count++] = 0;
                }
            }
            return count;
        }

        /// <summary>
        /// Удалить чанк
        /// </summary>
        public bool IsRemoved() => FlagsYAreas == 0;

        /// <summary>
        /// Количество замеряемых чанков, нужен для Finish
        /// </summary>
        public int GetQuantity() => FlagsYAreas;

        public void ReadPacket(ReadPacket stream)
        {
            CurrentChunkX = stream.Int();
            CurrentChunkY = stream.Int();
            FlagsYAreas = stream.Int();
            if (FlagsYAreas > 0)
            {
                IsBiom = stream.Bool();
                // Возвращаем данные без декомпрессии, чтоб объём данных был мал, 
                // декомпрессия будет в игровом клиентском потоке
                BufferRead = stream.Bytes();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(CurrentChunkX);
            stream.Int(CurrentChunkY);
            stream.Int(FlagsYAreas);
            if (FlagsYAreas > 0)
            {
                stream.Bool(IsBiom);
                stream.BytesCompress(_bufferWrite, 0, _count);
            }
        }

        public void Dispose()
        {
            BufferRead = null;
            GC.SuppressFinalize(this);
        }
    }
}
