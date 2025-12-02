using Mvk2.World.Gen;
using System.Runtime.CompilerServices;

namespace Mvk2.World.Biome
{
    public class BiomeMountains : BiomeIsland
    {
        public BiomeMountains(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider) { }

        /// <summary>
        /// Генерация столба от 1 до 2 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel1_2(int xz, int yh, int level1, int level2)
        {
            for (int y = level1; y < level2; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
        }

        /// <summary>
        /// Генерация столба от 2 до 3 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel2_3(int xz, int yh, int level2, int level3)
        {
            // Местами прослойки
            for (int y = level2; y < level3; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
        }

        /// <summary>
        /// Генерация столба от 3 до 5 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel3_5(int xz, int yh, int level3, int level5)
            => _GenLevel(xz, yh, level3, level5);

        /// <summary>
        /// Генерация столба от 5 до 4 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel5_4(int xz, int yh, int level5, int level4)
            => _GenLevel(xz, yh, level5, level4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _GenLevel(int xz, int yh, int level3, int level5)
        {
            int y;
            if (level5 == yh)
            {
                // Доп шум для перехода песка
                int noise = (_noise + 2) * 2;
                if (yh > 88 + noise)
                {
                    for (y = level3; y <= yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdStone);
                }
                else
                {
                    int l6 = level5 - noise;
                    for (y = level3; y < l6; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdStone);
                    for (y = l6; y < level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLoam);
                    _chunkPrimer.SetBlockState(xz, yh, _blockIdTurfLoam);
                }
            }
            else
            {
                for (y = level3; y < level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdStone);
            }

            //for (int y = level5; y < level4; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdStone);
            //if (level4 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdStone);
        }

        /// <summary>
        /// Генерация столба верхнего уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevelUp(int xz, int yh, int level)
            => _GenLevel(xz, yh, level, yh);
        //{
        //    // Камень
        //    for (int y = level; y <= yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdStone);
        //}
    }
}
