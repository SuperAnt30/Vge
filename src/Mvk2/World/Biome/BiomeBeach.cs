using Mvk2.World.Gen;
using System.Runtime.CompilerServices;

namespace Mvk2.World.Biome
{
    public class BiomeBeach : BiomeIsland
    {
        public BiomeBeach(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider) { }

        /// <summary>
        /// Генерация столба от 2 до 3 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel2_3(int xz, int yh, int level2, int level3)
        {
            // Местами прослойки песка между глиной и суглинком
            for (int y = level2; y < level3; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
            if (level3 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdSand);
        }

        /// <summary>
        /// Генерация столба от 3 до 5 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel3_5(int xz, int yh, int level3, int level5)
        {
            int y;
            if (level5 == yh)
            {
                // Доп шум для перехода песка
                int l6 = level5 - _noise - 1;
                for (y = level3; y < l6; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLoam);
                for (y = l6; y <= level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
            }
            else
            {
                for (y = level3; y < level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLoam);
            }
        }

        /// <summary>
        /// Генерация столба от 5 до 4 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel5_4(int xz, int yh, int level5, int level4)
        {
            // Местами прослойки глины между вверхном и суглинком
            for (int y = level5; y < level4; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdClay);
            // Сверху глина
            if (level4 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdClay);
        }

        /// <summary>
        /// Генерация столба верхнего уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevelUp(int xz, int yh, int level)
        {
            // Гравий
            for (int y = level; y <= yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdGravel);
        }

        /// <summary>
        /// Генерация столба поверхности, трава, цветы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevelSurface(int xz, int yh)
        {
            ushort b = _chunkPrimer.GetBlockId(xz, yh);
            if (b == _blockIdTurf || b == _blockIdTurfLoam)
            {
                base._GenLevelSurface(xz, yh);
            }
        }
    }
}
