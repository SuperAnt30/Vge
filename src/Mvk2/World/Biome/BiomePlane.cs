using Mvk2.World.Gen;
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
                new FeatureMinable(_chunkPrimer, 5, _blockIdWater, 33, 10), // Пресная вода
                //new FeatureValun(_chunkPrimer, 1, 1, _blockIdStone, 4, 1, 2, 1, 0)
            };
        }
    }
}
