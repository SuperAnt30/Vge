using Mvk2.World.Gen;
using Vge.World.Gen;

namespace Mvk2.World.Biome
{
    /// <summary>
    /// Сешанный лес
    /// </summary>
    public class BiomeMixedForest : BiomeIsland
    {
        public BiomeMixedForest(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider)
        {
            _featureAreas = new IFeatureGeneratorArea[]
            {
                // Берёза
                chunkProvider.Tree.CreateBirrchGen(1, 3),
                // Дуб
                chunkProvider.Tree.CreateOakGen(1, 3),
            };
        }

    }
}
