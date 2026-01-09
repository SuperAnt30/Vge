using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Отправляем клиенту много изменённых блоков
    /// </summary>
    public struct PacketS22MultiBlockChange : IPacket
    {
        public byte Id => 0x22;

        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public int CurrentChunkX { get; private set; }
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public int CurrentChunkY { get; private set; }

        private BlockUpdateData[] _blocks;

        public PacketS22MultiBlockChange(int count, int[] blocksLocalPos, ChunkBase chunk)
        {
            CurrentChunkX = chunk.CurrentChunkX;
            CurrentChunkY = chunk.CurrentChunkY;
            _blocks = new BlockUpdateData[count];
            int x, y, z, index;
            for (int i = 0; i < count; i++)
            {
                index = blocksLocalPos[i];
                x = (index >> 4) & 15;
                y = index >> 8;
                z = index & 15;

                _blocks[i] = new BlockUpdateData()
                {
                    Index = index,
                    State = chunk.GetBlockStateNotCheck(x, y, z)
                };
            }
        }

        /// <summary>
        /// Количество изменяемых блоков
        /// </summary>
        public int Count() => _blocks.Length;

        /// <summary>
        /// Получили блоки
        /// </summary>
        public void ReceivedBlocks(WorldBase world)
        {
            ChunkBase chunk = world.GetChunk(CurrentChunkX, CurrentChunkY);
            if (chunk != null)
            {
                BlockPos blockPos = new BlockPos();
                int chx = CurrentChunkX << 4;
                int chz = CurrentChunkY << 4;
                int index;
                for (int i = 0; i < _blocks.Length; i++)
                {
                    index = _blocks[i].Index;
                    blockPos.X = chx + ((index >> 4) & 15);
                    blockPos.Y = index >> 8;
                    blockPos.Z = chz + (index & 15);
                    chunk.SetBlockState(blockPos, _blocks[i].State, 20);
                }
            }
        }

        public void ReadPacket(ReadPacket stream)
        {
            CurrentChunkX = stream.Int();
            CurrentChunkY = stream.Int();
            ushort count = stream.UShort();
            _blocks = new BlockUpdateData[count];
            for (int i = 0; i < count; i++)
            {
                _blocks[i] = new BlockUpdateData()
                {
                    Index = stream.Int()
                };
                _blocks[i].State.ReadStream(stream);
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(CurrentChunkX);
            stream.Int(CurrentChunkY);
            stream.UShort((ushort)_blocks.Length);
            for (int i = 0; i < _blocks.Length; i++)
            {
                stream.Int(_blocks[i].Index);
                _blocks[i].State.WriteStream(stream);
            }
        }

        private struct BlockUpdateData
        {
            public int Index;
            public BlockState State;
        }
    }
}
