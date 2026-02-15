using Mvk2.World.Gen;
using System.Runtime.CompilerServices;
using Vge.World.Gen;
using Vge.World.Gen.Feature;

namespace Mvk2.World.Biome
{
    public class BiomeMountains : BiomeIsland
    {
        public BiomeMountains(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider) { }

        protected override void _InitFeature()
        {
            // Сначало надо те которые меняют блоки камня (блинчики, руда), они ставят флаг, если их использовать после, будет пустота
            // Потом используем те, которые добавляют, трава, деревья и прочее

            _featureColumns = new IFeatureGeneratorColumn[]
            {
                new FeatureOreSprinkleUp(_chunkPrimer, 1, 5, 1, 3, _blockIdOreSulfur, _blockIdLimestone),
                new FeatureOreSprinkleUp(_chunkPrimer, 5, 4, 8, _blockIdOreGold, _blockIdLimestone),
                new FeatureOreSprinkle(_chunkPrimer, 5, 5, 1, 3, _blockIdOreDiamond, _blockIdGranite, 10, 64),
                new FeatureOreSprinkle(_chunkPrimer, 5, 5, 1, 3, _blockIdOreSapphire, _blockIdGranite, 10, 64),
                new FeatureOreSprinkle(_chunkPrimer, 2, 5, 1, 2, _blockIdOreEmerald, _blockIdGranite, 10, 64),
                new FeatureOreSprinkle(_chunkPrimer, 2, 5, 1, 2, _blockIdOreRuby, _blockIdGranite, 10, 64),
                new FeatureOreSprinkle(_chunkPrimer, 1, 3, 2, 6, _blockIdOreGold, _blockIdLimestone, 30, 96),
            };

            _featureAreas = new IFeatureGeneratorArea[]
            {

            };

            _featureColumnsAfter = new IFeatureGeneratorColumn[]
            {
            };
        }

        /// <summary>
        /// Генерация столба от около 1 много
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel1Many(int xz, int yh, int level0, int level1)
        {
            for (int y = level0; y < level1; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdOreIron);
            if (level1 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdOreIron);
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
                    for (y = level3; y <= yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
                }
                else
                {
                    int l6 = level5 - noise;
                    for (y = level3; y < l6; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
                    for (y = l6; y < level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLoam);
                    _chunkPrimer.SetBlockState(xz, yh, _blockIdTurfLoam);
                }
            }
            else
            {
                for (y = level3; y < level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
            }
        }

        /// <summary>
        /// Генерация столба верхнего уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevelUp(int xz, int yh, int level)
            => _GenLevel(xz, yh, level, yh);

        /// <summary>
        /// Генерация столба поверхности, трава, цветы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevelSurface(int xz, int yh)
        {
            if (_noise17 < -1)
            {
                int b = _chunkPrimer.GetBlockId(xz, yh);
                if (b == _blockIdTurf || b == _blockIdTurfLoam)
                {
                    _chunkPrimer.SetBlockState(xz, yh + 1, _blockIdGrass);
                }
            }
        }
    }
}
