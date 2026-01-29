using System.Runtime.CompilerServices;

namespace Vge.World.Block
{
    /// <summary>
    /// Локальная позиция блока с Id, безм мет данных, для генерации и BlockEntity
    /// В 2 инта
    /// </summary>
    public struct BlockPosLoc
    {
        /// <summary>
        /// Позиция 
        /// Тут позиция от текущего чанко, где зараждена основа.
        /// т.е. -15..[0..15]..30 (в чанке старт и в любую сторону до 15) 45, нам надо 6 бит.
        /// y << 12 | z << 6 | x
        /// ---- ---- | ---y yyyy | yyyy zzzz | zzxx xxxx
        /// 4 байта, т.е. int
        /// </summary>
        private readonly int _position;
        /// <summary>
        /// Индекс блока
        /// </summary>
        public readonly int Id;

        public readonly int ParentId;

        public BlockPosLoc(int x, int y, int z, int id, int parent)
        {
            _position = y << 12 | (z + 16) << 6 | (x + 16);
            Id = id;
            ParentId = parent;
        }

        public BlockPosLoc(BlockCache blockCache)
        {
            _position = blockCache.Position.Y << 12 | (blockCache.Position.Z + 16) << 6 | (blockCache.Position.X + 16);
            Id = blockCache.Id;
            ParentId = blockCache.Parent;
        }

        public BlockPosLoc(BlockPos blockPos, int id, int parent)
        {
            _position = blockPos.Y << 12 | (blockPos.Z + 16) << 6 | (blockPos.X + 16);
            Id = id;
            ParentId = parent;
        }

        public BlockPosLoc(BlockPosLoc posLoc, int parent)
        {
            _position = posLoc._position;
            Id = posLoc.Id;
            ParentId = parent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsPos(int posLos) => _position == posLos;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetX() => (_position & 63) - 16;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetY() => _position >> 12;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetZ() => ((_position >> 6) & 63) - 16;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos GetBlockPos() => new BlockPos(GetX(), GetY(), GetZ());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos GetBlockPos(int x, int z) => new BlockPos(GetX() + x, GetY(), GetZ() + z);

        public override string ToString()
            => "Id:" + Id + " [" + GetX() + ", " + GetY() + ", " + GetZ() + "] P:" + ParentId;
    }
}
