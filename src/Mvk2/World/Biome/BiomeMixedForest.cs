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
                chunkProvider.Tree.CreateBirrchGen(0, 1),
                // Дуб
                chunkProvider.Tree.CreateOakGen(0, 1),
                // Фруктовое дерева
                chunkProvider.Tree.CreateFruitGen(0, 1),
                // Хвойное
                chunkProvider.Tree.CreateConiferGen(0, 1)
            };
        }

    }
}
