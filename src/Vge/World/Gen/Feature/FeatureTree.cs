using Vge.Util;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.World.Gen.Feature
{
    /// <summary>
    /// Генерация дерева
    /// </summary>
    public class FeatureTree : FeatureArea
    {
        /// <summary>
        /// Количество блоков менерала
        /// </summary>
        private readonly byte _count;
        /// <summary>
        /// Минимальный по уровню Y
        /// </summary>
        private readonly byte _minY;
        /// <summary>
        /// Диапазон по Y
        /// </summary>
        private readonly byte _rangeY;

        private readonly int _blockBranchId;
        private readonly int _blockLeavesId;

        public FeatureTree(IChunkPrimer chunkPrimer, byte minRandom, byte maxRandom, 
            int blockLogId, int blockBranchId, int blockLeavesId) 
            : base(chunkPrimer, minRandom, maxRandom, blockLogId)
        {
            _blockBranchId = blockBranchId;
            _blockLeavesId = blockLeavesId;
            //_minY = minY;
            //_isAir = _minY == 255;
            //_flagAir = (byte)(_isAir ? 0 : 1);
            //_rangeY = _isAir ? maxY : (byte)(maxY - minY);
            //_count = count;
        }

        public FeatureTree(IChunkPrimer chunkPrimer, byte probabilityOne,
            int blockId) : base(chunkPrimer, probabilityOne, blockId)
        {
            //_rangeY = _isAir ? maxY : (byte)(maxY - minY);
            //_count = count;
        }

        /// <summary>
        /// Декорация области одного прохода
        /// </summary>
        protected override void _DecorationAreaOctave(ChunkServer chunkSpawn, Rand rand)
        {
            int x0 = rand.Next(16);
            int z0 = rand.Next(16);
            int y0 = chunkSpawn.HeightMapGen[z0 << 4 | x0] - _rangeY;

                
            _SetBlockReplace(x0, y0, z0, _blockId, 0);
            _SetBlockReplace(x0, y0 + 1, z0, _blockId, 0);
            _SetBlockReplace(x0, y0 + 2, z0, _blockId, 0);

            for (int x = x0 - 8; x < x0 + 9; x++)
            {
                _SetBlockState(x, y0 + 3, z0, _blockBranchId, 1);
            }
            _SetBlockState(x0, y0 + 3, z0, _blockId, 0);
            for (int z = z0 - 8; z < z0 + 9; z++)
            {
                _SetBlockState(x0, y0 + 4, z, _blockBranchId, 2);
            }
            _SetBlockState(x0, y0 + 3, z0 - 4, _blockLeavesId, 1);
            _SetBlockState(x0, y0 + 5, z0 - 4, _blockLeavesId, 0);

            _SetBlockState(x0 + 1, y0 + 6, z0, _blockLeavesId, 2);
            _SetBlockState(x0 - 1, y0 + 6, z0, _blockLeavesId, 3);
            _SetBlockState(x0, y0 + 6, z0 - 1, _blockLeavesId, 4);
            _SetBlockState(x0, y0 + 6, z0 + 1, _blockLeavesId, 5);

            _SetBlockState(x0, y0 + 4, z0, _blockId, 0);
            _SetBlockState(x0, y0 + 5, z0, _blockId, 0);
            _SetBlockState(x0, y0 + 6, z0, _blockId, 0);
            _SetBlockState(x0, y0 + 7, z0, _blockId, 0);
            _SetBlockState(x0, y0 + 8, z0, _blockBranchId, 0);
            _SetBlockState(x0, y0 + 9, z0, _blockBranchId, 4);
            _SetBlockState(x0, y0 + 10, z0, _blockBranchId, 0);

            _SetBlockState(x0 + 1, y0 + 8, z0, _blockLeavesId, 2);
            _SetBlockState(x0 + 1, y0 + 10, z0, _blockLeavesId, 2);
            _SetBlockState(x0 - 1, y0 + 10, z0, _blockLeavesId, 3);
            _SetBlockState(x0, y0 + 10, z0 - 1, _blockLeavesId, 4);
            _SetBlockState(x0, y0 + 10, z0 + 1, _blockLeavesId, 5);

            _SetBlockState(x0, y0 + 11, z0, _blockLeavesId, 0);
        }
    }
}
