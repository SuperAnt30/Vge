using System;
using System.Collections.Generic;
using Vge.Util;
using Vge.World.Chunk;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Отправляем клиенту изменённые псевдо чанки
    /// </summary>
    public struct PacketS21ChunkData : IPacket
    {
        public byte Id => 0x21;

        /// <summary>
        /// Статический быстрый буфер для записи
        /// </summary>
        private static readonly ListByte _bufferWrite = new ListByte(32768);

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
        public uint FlagsYAreas { get; private set; }
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
        public PacketS21ChunkData(ChunkBase chunk, bool biom, uint flagsYAreas)
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

            ushort data;
            int i, y;
            uint value;
            ChunkStorage chunkStorage;
            _bufferWrite.Clear();
            for (y = 0; y < chunk.NumberSections; y++)
            {
                if ((flagsYAreas & 1 << y) != 0)
                {
                    chunkStorage = chunk.StorageArrays[y];

                    _bufferWrite.AddRange(chunkStorage.Light);

                    if (chunkStorage.IsEmptyData())
                    {
                        _bufferWrite.Add(0);
                    }
                    else
                    {
                        _bufferWrite.Add(1);
                        _bufferWrite.AddRange(chunkStorage.Data);

                        data = (ushort)chunkStorage.Metadata.Count;
                        _bufferWrite.Add((byte)(data >> 8));
                        _bufferWrite.Add((byte)(data & 0xFF));

                        foreach (KeyValuePair<ushort, uint> entry in chunkStorage.Metadata)
                        {
                            _bufferWrite.Add((byte)(entry.Key >> 8));
                            _bufferWrite.Add((byte)(entry.Key & 0xFF));
                            value = entry.Value;
                            _bufferWrite.Add((byte)((value & 0xFF000000) >> 24));
                            _bufferWrite.Add((byte)((value & 0xFF0000) >> 16));
                            _bufferWrite.Add((byte)((value & 0xFF00) >> 8));
                            _bufferWrite.Add((byte)(value & 0xFF));
                        }
                    }
                }
            }
            if (biom)
            {
                // добавляем данные биома
                for (i = 0; i < 256; i++)
                {
                    _bufferWrite.Add(0);
                }
            }
        }

        /// <summary>
        /// Удалить чанк
        /// </summary>
        public bool IsRemoved() => FlagsYAreas == 0;

        /// <summary>
        /// Количество замеряемых чанков, нужен для Finish
        /// </summary>
        //public int GetQuantity() => FlagsYAreas;

        public void ReadPacket(ReadPacket stream)
        {
            CurrentChunkX = stream.Int();
            CurrentChunkY = stream.Int();
            FlagsYAreas = stream.UInt();
            if (FlagsYAreas > 0)
            {
                IsBiom = stream.Bool();
                // Возвращаем данные без декомпрессии, чтоб объём данных был мал, 
                // декомпрессия будет в игровом клиентском потоке
                BufferRead = stream.Bytes();
                //BufferRead = stream.BytesDecompress();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(CurrentChunkX);
            stream.Int(CurrentChunkY);
            stream.UInt(FlagsYAreas);
            if (FlagsYAreas > 0)
            {
                stream.Bool(IsBiom);
                stream.BytesCompress(_bufferWrite.GetBufferAll(), 0, _bufferWrite.Count);
            }
        }
    }
}
