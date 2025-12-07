using Mvk2.World.Gen;
using System.Runtime.CompilerServices;
using Vge.World.Gen;
using Vge.World.Gen.Feature;

namespace Mvk2.World.Biome
{
    /// <summary>
    /// Хвойный лес
    /// </summary>
    public class BiomeConiferousForest : BiomeIsland
    {
        public BiomeConiferousForest(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider)
        {
            _featureAreas = new IFeatureGeneratorArea[]
            {
                new FeatureMinable(_chunkPrimer, 10, _blockIdStone, 33, 6), // Валун ок
                new FeatureValun(_chunkPrimer, 20, _blockIdStone, 4, 1, 2, 1, 0)
            };
        }

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
