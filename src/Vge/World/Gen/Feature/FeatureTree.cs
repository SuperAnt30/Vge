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

        public FeatureTree(IChunkPrimer chunkPrimer, byte minRandom, byte maxRandom, ushort blockId) 
            : base(chunkPrimer, minRandom, maxRandom, blockId)
        {
            //_minY = minY;
            //_isAir = _minY == 255;
            //_flagAir = (byte)(_isAir ? 0 : 1);
            //_rangeY = _isAir ? maxY : (byte)(maxY - minY);
            //_count = count;
        }

        public FeatureTree(IChunkPrimer chunkPrimer, byte probabilityOne, 
            ushort blockId) : base(chunkPrimer, probabilityOne, blockId)
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
                _SetBlockState(x, y0 + 3, z0, _blockId, 1);
            }
            for (int z = z0 - 8; z < z0 + 9; z++)
            {
                _SetBlockState(x0, y0 + 4, z, _blockId, 2);
            }
            _SetBlockReplace(x0, y0 + 5, z0, _blockId, 0);
        }
    }
}
