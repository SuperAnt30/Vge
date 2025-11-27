using Mvk2.World.Block;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Gen;

namespace Mvk2.World.Gen.Feature
{
    public class FeatureCactus : IFeatureGeneratorColumn
    {
        protected ChunkPrimerIsland _chunkPrimer;

        /// <summary>
        /// Вероятность одной
        /// </summary>
        private int _probabilityOne;

        public FeatureCactus(ChunkPrimerIsland chunkPrimer, int probabilityOne)
        {
            _chunkPrimer = chunkPrimer;
            _probabilityOne = probabilityOne;
        }

        /// <summary>
        /// Декорация блока или столба не выходящего за чанк
        /// </summary>
        public void DecorationsColumn(ChunkBase chunkSpawn, Rand rand)
        {
            if (rand.Next(_probabilityOne) == 0)
            {
                int xz = rand.Next(16) << 4 | rand.Next(16);
                int cloum = rand.Next(4) + 3;
                int y = chunkSpawn.HeightMapGen[xz];
                if (y > 0)
                {
                    BlockState blockState = chunkSpawn.GetBlockStateNotCheckLight(xz, y - 1);
                    if (blockState.Id == BlocksRegMvk.Granite.IndexBlock)
                    {
                        for (int y2 = 0; y2 < cloum; y2++)
                        {
                            _chunkPrimer.SetBlockStateFlag(xz, y + y2, BlocksRegMvk.Limestone.IndexBlock, 0);
                        }
                    }
                }
            }
        }
    }
}
