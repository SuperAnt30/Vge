using Vge.Util;
using Vge.World.Chunk;

namespace Vge.World.Gen.Feature
{
    /// <summary>
    /// Генерация особенностей, блинчик
    /// </summary>
    public class FeaturePancake : FeatureArea
    {
        /// <summary>
        /// Радиус блинчика
        /// </summary>
        private readonly byte _radius;
        /// <summary>
        /// Высота блинчика
        /// </summary>
        private readonly byte _height;

        public FeaturePancake(IChunkPrimer chunkPrimer, byte minRandom, byte maxRandom, 
            ushort blockId, byte radius, byte height) : base(chunkPrimer, minRandom, maxRandom, blockId)
        {
            _radius = radius;
            _height = height;
        }

        /// <summary>
        /// Декорация области одного прохода
        /// </summary>
        protected override void _DecorationAreaOctave(ChunkBase chunkSpawn, Rand rand)
        {
            int x0 = rand.Next(16);
            int z0 = rand.Next(16);
            // Генератором уменьшаем радиус блинчика
            int size = rand.Next(_radius) + 4;
            // Высота блинчика 2 - 7
            int h = rand.Next(_height) + 2;
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
                                _SetBlockReplace(x, y, z, _blockId);
                            }
                        }
                    }
                }
            }
        }
    }
}
