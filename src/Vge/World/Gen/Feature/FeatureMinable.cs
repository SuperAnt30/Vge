using Vge.Util;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.World.Gen.Feature
{
    /// <summary>
    /// Генерация особенностей, повёрнутый эллипс 3д
    /// </summary>
    public class FeatureMinable : FeatureArea
    {
        /// <summary>
        /// Количество блоков менерала
        /// </summary>
        private readonly byte _count;
        /// <summary>
        /// Минимальный по уровню Y
        /// </summary>
        private readonly byte _minY;
        /// <summary>
        /// Диапазон по Y
        /// </summary>
        private readonly byte _rangeY;
        /// <summary>
        /// Можно ли в воздухе ставить
        /// </summary>
        private readonly bool _isAir;
        /// <summary>
        /// Флаг для установки в воздухе
        /// </summary>
        private readonly byte _flagAir;

        public FeatureMinable(IChunkPrimer chunkPrimer, byte minRandom, byte maxRandom,
            ushort blockId, byte count, byte rangeY) : this(chunkPrimer, minRandom, maxRandom, blockId,
                count, 255, rangeY) { }

        public FeatureMinable(IChunkPrimer chunkPrimer, byte minRandom, byte maxRandom,
            ushort blockId, byte count, byte minY, byte maxY) : base(chunkPrimer, minRandom, maxRandom, blockId)
        {
            _minY = minY;
            _isAir = _minY == 255;
            _flagAir = (byte)(_isAir ? 0 : 1);
            _rangeY = _isAir ? maxY : (byte)(maxY - minY);
            _count = count;
        }

        public FeatureMinable(IChunkPrimer chunkPrimer, byte probabilityOne,
            ushort blockId, byte count, byte rangeY) : this(chunkPrimer, probabilityOne, blockId,
                count, 255, rangeY) { }

        public FeatureMinable(IChunkPrimer chunkPrimer, byte probabilityOne, 
            ushort blockId, byte count, byte minY, byte maxY) : base(chunkPrimer, probabilityOne, blockId)
        {
            _minY = minY;
            _isAir = _minY == 255;
            _flagAir = (byte)(_isAir ? 0 : 1);
            _rangeY = _isAir ? maxY : (byte)(maxY - minY);
            _count = count;
        }

        /// <summary>
        /// Декорация области одного прохода
        /// </summary>
        protected override void _DecorationAreaOctave(ChunkBase chunkSpawn, Rand rand)
        {
            int x0 = rand.Next(16);
            int z0 = rand.Next(16);
            int y0 = _isAir
                ? chunkSpawn.HeightMapGen[z0 << 4 | x0] - _rangeY
                : (_rangeY != 0 ? rand.Next(_rangeY) + _minY : _minY);

            float k, x3, y3, z3, c, h, v, xk, yk, zk;
            int x4, y4, z4, x5, y5, z5, i, x, y, z;
            float f = rand.NextFloat() * Glm.Pi;

            float x1 = x0 + 8f + Glm.Sin(f) * _count / 8f;
            float x2 = x0 + 8f - Glm.Sin(f) * _count / 8f;
            float z1 = z0 + 8f + Glm.Cos(f) * _count / 8f;
            float z2 = z0 + 8f - Glm.Cos(f) * _count / 8f;
            float y1 = y0 + rand.Next(3) - 2;
            float y2 = y0 + rand.Next(3) - 2;

            for (i = 0; i < _count; ++i)
            {
                k = i / (float)_count;
                x3 = x1 + (x2 - x1) * k;
                y3 = y1 + (y2 - y1) * k;
                z3 = z1 + (z2 - z1) * k;
                c = rand.NextFloat() * _count / 16f;
                h = (Glm.Sin(Glm.Pi * k) + 1f) * c + 1f;
                v = (Glm.Sin(Glm.Pi * k) + 1f) * c + 1f;
                x4 = Mth.Floor(x3 - h / 2f);
                y4 = Mth.Floor(y3 - v / 2f);
                z4 = Mth.Floor(z3 - h / 2f);
                x5 = Mth.Floor(x3 + h / 2f);
                y5 = Mth.Floor(y3 + v / 2f);
                z5 = Mth.Floor(z3 + h / 2f);

                for (x = x4; x <= x5; ++x)
                {
                    xk = (x + .5f - x3) / (h / 2f);
                    if (xk * xk < 1f)
                    {
                        for (y = y4; y <= y5; ++y)
                        {
                            yk = (y + .5f - y3) / (v / 2f);
                            if (xk * xk + yk * yk < 1f)
                            {
                                for (z = z4; z <= z5; ++z)
                                {
                                    zk = (z + .5f - z3) / (h / 2f);
                                    if (xk * xk + yk * yk + zk * zk < 1f && y > 3)
                                    {
                                        _SetBlockReplace(x, y, z, _blockId, _flagAir);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
