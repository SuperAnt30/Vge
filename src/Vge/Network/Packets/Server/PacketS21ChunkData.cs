using System;
using System.Collections.Generic;
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
        /// Количество буфер данных псевдо чанка.
        /// 16 * 16 * 16 * 3 + 2 на доп метданных
        /// </summary>
        private int _CountBufChunck() => 12290;
        /// <summary>
        /// Количество буфер данных для биома чанка.
        /// 16 * 16
        /// </summary>
        private int _CountBufBiom() => IsBiom ? 256 : 0;

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
            chunk.GenBinary(biom, flagsYAreas);
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
                BufferRead = stream.BytesDecompress();
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
                stream.BytesCompress(ChunkBase.BufferWrite.GetBufferAll(), 0, ChunkBase.BufferWrite.Count);
            }
        }
    }
}
