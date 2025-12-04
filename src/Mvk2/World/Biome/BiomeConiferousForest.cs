using Mvk2.World.Gen;
using System.Runtime.CompilerServices;

namespace Mvk2.World.Biome
{
    /// <summary>
    /// Хвойный лес
    /// </summary>
    public class BiomeConiferousForest : BiomeIsland
    {
        public BiomeConiferousForest(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider) { }

        /// <summary>
        /// Генерация столба от 5 до 4 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel5_4(int xz, int yh, int level5, int level4)
        {
            // Местами прослойки глины между вверхном и суглинком
            for (int y = level5; y < level4; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
            // Сверху песок
            if (level4 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdSand);
        }

        /// <summary>
        /// Генерация столба верхнего уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevelUp(int xz, int yh, int level)
        {
            // Песок
            for (int y = level; y < yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
            _chunkPrimer.SetBlockState(xz, yh, yh < HeightWater ? _blockIdLoam : _blockIdTurfLoam);
        }
    }
}
