using Mvk2.World.Gen;
using System.Runtime.CompilerServices;

namespace Mvk2.World.Biome
{
    public class BiomeSea : BiomeIsland
    {
        public BiomeSea(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider) { }

        /// <summary>
        /// Генерация столба от 0 до 1 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel0_1(int xz, int yh, int level0, int level1)
        {
            int y;
            if (level1 == yh)
            {
                // Доп шум для перехода гравия
                int l1 = level1 - _noise - 1;
                for (y = level0; y < l1; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
                for (y = l1; y <= level1; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdGravel);
            }
            else
            {
                for (y = level0; y < level1; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
            }
        }

        /// <summary>
        /// Генерация столба от 1 до 2 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel1_2(int xz, int yh, int level1, int level2)
        {
            for (int y = level1; y < level2; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
            if (level2 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdLimestone);
        }

        /// <summary>
        /// Генерация столба от 2 до 3 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel2_3(int xz, int yh, int level2, int level3)
        {
            for (int y = level2; y < level3; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
            if (level3 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdLimestone);
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
                for (y = level3; y < l6; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
                for (y = l6; y <= level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
            }
            else
            {
                for (y = level3; y < level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdGravel);
            }
        }

        /// <summary>
        /// Генерация столба от 5 до 4 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel5_4(int xz, int yh, int level5, int level4)
        {
            for (int y = level5; y < level4; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdGravel);
            // Сверху гравий
            if (level4 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdGravel);
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
    }
}
