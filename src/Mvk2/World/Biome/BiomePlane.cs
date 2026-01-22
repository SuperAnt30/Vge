using Mvk2.World.Gen;
using Mvk2.World.Gen.Feature;
using System.Runtime.CompilerServices;
using Vge.World.Gen;
using Vge.World.Gen.Feature;

namespace Mvk2.World.Biome
{
    public class BiomePlane : BiomeIsland
    {
        public BiomePlane(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider)
        {
            _featureAreas = new IFeatureGeneratorArea[]
            {
               // new FeaturePancake(_chunkPrimer, 1, 1, _blockIdStone, 6, 8),
                //new FeatureMinable(_chunkPrimer, 0, 3, _blockIdStone, 33, 0)
                
                new FeatureMinable(_chunkPrimer, 10, _blockIdStone, 33, 6), // Валун ок
                new FeatureMinable(_chunkPrimer, 5, _blockIdWater, 33, 24, 44), // Пресная вода
                //new FeatureTree(_chunkPrimer, 1, 2, _blockIdLogBirch), // Дерево
                // Берёза
                new FeatureTreeBirch(chunkProvider.BlockCaches, _chunkPrimer), // Дерево
                // Дуб
                new FeatureTreeOak(chunkProvider.BlockCaches,_chunkPrimer), // Дерево
                
                //new FeatureValun(_chunkPrimer, 1, 1, _blockIdStone, 4, 1, 2, 1, 0)
            };
        }

        /// <summary>
        /// Генерация столба от около 1 много
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel1Many(int xz, int yh, int level0, int level1)
        {
            for (int y = level0; y < level1; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdOreCoal);
            if (level1 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdOreCoal);
        }
    }
}
