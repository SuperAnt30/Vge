using Vge.World.Chunk;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Отправляем клиенту изменённые псевдо чанки
    /// </summary>
    public struct PacketS21ChunkData : IPacket
    {
        public byte GetId() => 0x21;

        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public int CurrentChunkX { get; private set; }
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public int CurrentChunkY { get; private set; }
        /// <summary>
        /// Флаг псевдо чанков
        /// </summary>
        public ushort FlagsYAreas { get; private set; }

        /// <summary>
        /// Выгрузить чанк у игрока
        /// </summary>
        public PacketS21ChunkData(int chunkPosX, int chunkPosY)
        {
            CurrentChunkX = chunkPosX;
            CurrentChunkY = chunkPosY;
            FlagsYAreas = 0;
        }
        /// <summary>
        /// Загрузка данных чанка
        /// </summary>
        public PacketS21ChunkData(ChunkBase chunk, ushort flagsYAreas)
        {
            CurrentChunkX = chunk.CurrentChunkX;
            CurrentChunkY = chunk.CurrentChunkY;
            FlagsYAreas = flagsYAreas;
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
            FlagsYAreas = stream.UShort();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(CurrentChunkX);
            stream.Int(CurrentChunkY);
            stream.UShort(FlagsYAreas);
        }
    }
}
