using Mvk2.World.Block;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Gen;

namespace Mvk2.World.Gen.Feature
{
    public class FeaturePancake : IFeatureGeneratorArea
    {
        protected ChunkPrimerIsland _chunkPrimer;
        /// <summary>
        /// Радиус блинчика
        /// </summary>
        private readonly int _radius = 9;
        /// <summary>
        /// Высота блинчика
        /// </summary>
        private readonly int _height = 7;
        /// <summary>
        /// Смещение чанка по X, для определения чанка в какой сетим
        /// </summary>
        private int _biasX;
        /// <summary>
        /// Смещение чанка по Z, для определения чанка в какой сетим
        /// </summary>
        private int _biasZ;

        public FeaturePancake(ChunkPrimerIsland chunkPrimer) => _chunkPrimer = chunkPrimer;

        /// <summary>
        /// Декорация областей которые могу выйти за 1 чанк
        /// </summary>
        public void DecorationsArea(ChunkBase chunkSpawn, Rand rand, int biasX, int biasZ)
        {
            if (rand.Next(5) != 0) return;
            _biasX = biasX;
            _biasZ = biasZ;

            int x0 = rand.Next(16);
            int z0 = rand.Next(16);

            // Генератором уменьшаем радиус блинчика
            int size = rand.Next(_radius - 4) + 4;
            // Высота блинчика 2 - 7
            int h = rand.Next(_height - 3) + 2;
            // Центр блинчика от точки старта
            int center = rand.Next(h);

            int xz = z0 << 4 | x0;
            // Определение высоты ландшафта если надо
            int y0 = chunkSpawn.HeightMapGen[xz];
            if (y0 > 0)
            {
                int x, y, z, x1, z1;
                int ymin = y0 - center;
                int ymax = ymin + h;
                int check = size * size + 1;

                for (x = x0 - size; x <= x0 + size; ++x)
                {
                    x1 = x - x0;
                    for (z = z0 - size; z <= z0 + size; ++z)
                    {
                        z1 = z - z0;
                        if (x1 * x1 + z1 * z1 <= check)
                        {
                            for (y = ymin; y <= ymax; ++y)
                            {
                                _SetBlockReplace(x, y, z, BlocksRegMvk.Stone.IndexBlock);
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SetBlockReplace(int x, int y, int z, ushort id)
        {
            if (_biasX == (x >> 4) && _biasZ == (z >> 4))
            {
                int xz = (z & 15) << 4 | (x & 15);
                _chunkPrimer.SetBlockStateFlag(xz, y, id, 1);
            }
        }
    }
}
