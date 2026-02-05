using System.Runtime.CompilerServices;

namespace Vge.World.Block
{
    /// <summary>
    /// Локальная позиция блока с Id, безм мет данных, для генерации и BlockEntity
    /// </summary>
    public readonly struct BlockPosLoc
    {
        /// <summary>
        /// Позиция кеш для быстрого поиска
        /// Тут позиция от текущего чанко, где зараждена основа.
        /// т.е. -15..[0..15]..30 (в чанке старт и в любую сторону до 15) 45, нам надо 6 бит.
        /// y << 12 | z << 6 | x
        /// ---- ---- | ---y yyyy | yyyy zzzz | zzxx xxxx
        /// 4 байта, т.е. int
        /// </summary>
        private readonly int _posCache;
        /// <summary>
        /// Локальная позиция X смещения от текущего чанко, где зараждена основа
        /// </summary>
        public readonly int X;
        /// <summary>
        /// Локальная позиция X смещения от текущего чанко, где зараждена основа
        /// </summary>
        public readonly int Y;
        /// <summary>
        /// Локальная позиция X смещения от текущего чанко, где зараждена основа
        /// </summary>
        public readonly int Z;
        /// <summary>
        /// Индекс блока
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// Родительский Index в массиве к какому пренадлежит
        /// </summary>
        public readonly int ParentIndex;

        public BlockPosLoc(int x, int y, int z, int id, int parent)
        {
            X = x;
            Y = y;
            Z = z;
            _posCache = y << 12 | (z + 16) << 6 | (x + 16);
            Id = id;
            ParentIndex = parent;
        }

        public BlockPosLoc(BlockCache blockCache)
        {
            X = blockCache.Position.X;
            Y = blockCache.Position.Y;
            Z = blockCache.Position.Z;
            _posCache = Y << 12 | (Z + 16) << 6 | (X + 16);
            Id = blockCache.Id;
            ParentIndex = blockCache.ParentIndex;
        }

        public BlockPosLoc(BlockPos blockPos, int id, int parent)
        {
            X = blockPos.X;
            Y = blockPos.Y;
            Z = blockPos.Z;
            _posCache = Y << 12 | (Z + 16) << 6 | (X + 16);
            Id = id;
            ParentIndex = parent;
        }

        public BlockPosLoc(BlockPosLoc posLoc, int parent)
        {
            X = posLoc.X;
            Y = posLoc.Y;
            Z = posLoc.Z;
            _posCache = posLoc._posCache;
            Id = posLoc.Id;
            ParentIndex = parent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsPos(int posLos) => _posCache == posLos;

        /// <summary>
        /// Проверка текущих координат
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsPos(BlockPos pos) => X == pos.X && Y == pos.Y && Z == pos.Z;

        /// <summary>
        /// Получить позицию блока локальный
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos GetBlockPos() => new BlockPos(X, Y, Z);
        /// <summary>
        /// Получить позицию блока со смещением по x и z
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockPos GetBlockPos(int x, int z) => new BlockPos(X + x, Y, Z + z);

        public override string ToString()
            => "Id:" + Id + " [" + GetBlockPos() + "] P:" + ParentIndex;
    }
}
