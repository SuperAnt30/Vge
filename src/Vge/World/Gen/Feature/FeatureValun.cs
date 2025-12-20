using Vge.Util;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.World.Gen.Feature
{
    /// <summary>
    /// Генерация особенностей, валун
    /// </summary>
    public class FeatureValun : FeatureArea
    {
        /// <summary>
        /// Радиус по XZ
        /// </summary>
        private readonly byte _radiusXZ;
        /// <summary>
        /// Радиус по Y
        /// </summary>
        private readonly byte _radiusY;
        /// <summary>
        /// Радиус минимальный по XZ
        /// </summary>
        private readonly byte _radiusMinXZ;
        /// <summary>
        /// Радиус минимальный по Y
        /// </summary>
        private readonly byte _radiusMinY;
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

        public FeatureValun(IChunkPrimer chunkPrimer, byte minRandom, byte maxRandom, ushort blockId, 
            byte radiusXZ, byte radiusMinXZ, byte radiusY, byte radiusMinY, byte rangeY) 
            : this(chunkPrimer, minRandom, maxRandom, blockId, radiusXZ, radiusMinXZ, radiusY, radiusMinY, 255, rangeY) { }

        public FeatureValun(IChunkPrimer chunkPrimer, byte minRandom, byte maxRandom, ushort blockId, 
            byte radiusXZ, byte radiusMinXZ, byte radiusY, byte radiusMinY, byte minY, byte maxY) 
            : base(chunkPrimer, minRandom, maxRandom, blockId) 
        {
            _minY = minY;
            _isAir = _minY == 255;
            _flagAir = (byte)(_isAir ? 0 : 1);
            _radiusXZ = radiusXZ;
            _radiusMinXZ = radiusMinXZ;
            _radiusY = radiusY;
            _radiusMinY = radiusMinY;
        }

        public FeatureValun(IChunkPrimer chunkPrimer, byte probabilityOne, ushort blockId,
            byte radiusXZ, byte radiusMinXZ, byte radiusY, byte radiusMinY, byte rangeY)
            : this(chunkPrimer, probabilityOne, blockId, radiusXZ, radiusMinXZ, radiusY, radiusMinY, 255, rangeY) { }

        public FeatureValun(IChunkPrimer chunkPrimer, byte probabilityOne, ushort blockId,
            byte radiusXZ, byte radiusMinXZ, byte radiusY, byte radiusMinY, byte minY, byte maxY) 
            : base(chunkPrimer, probabilityOne, blockId)
        {
            _minY = minY;
            _isAir = _minY == 255;
            _flagAir = (byte)(_isAir ? 0 : 1);
            _radiusXZ = radiusXZ;
            _radiusMinXZ = radiusMinXZ;
            _radiusY = radiusY;
            _radiusMinY = radiusMinY;
        }

        /// <summary>
        /// Декорация области одного прохода
        /// </summary>
        protected override void _DecorationAreaOctave(ChunkServer chunkSpawn, Rand rand)
        {
            int x0 = rand.Next(16);
            int z0 = rand.Next(16);
            int y0 = _rangeY != 0 ? rand.Next(_rangeY) : 0;
            if (_isAir)
            {
                y0 += chunkSpawn.HeightMapGen[z0 << 4 | x0];
            }
            else
            {
                y0 += _minY;
            }

            int rmx = -rand.Next(_radiusXZ) - _radiusMinXZ;
            int rx = rand.Next(_radiusXZ) + _radiusMinXZ;
            int rmy = -rand.Next(_radiusXZ) - _radiusMinXZ;
            int ry = rand.Next(_radiusXZ) + _radiusMinXZ;
            int rmz = -rand.Next(_radiusY) - _radiusMinY;
            int rz = rand.Next(_radiusY) + _radiusMinY;
            
            float xk, zk;
            int y4, z4, y5, z5;

            for (int tx = rmx; tx <= rx; tx++)
            {
                xk = tx < 0 ? tx / (float)(rmx - 1) : tx / (float)(rx + 1);
                xk = 1 - xk * xk;

                z4 = Mth.Floor(rmz * xk);
                z5 = Mth.Ceiling(rz * xk);
                for (int tz = z4; tz <= z5; tz++)
                {
                    zk = tz < 0 ? tz / (float)(rmz - 1) : tz / (float)(rz + 1);
                    zk = 1 - zk * zk;
                    zk *= xk;

                    y4 = Mth.Floor(rmy * zk);
                    y5 = Mth.Ceiling(ry * zk);
                    for (int ty = y4; ty <= y5; ty++)
                    {
                        _SetBlockReplace(x0 + tx, y0 + ty, z0 + tz, _blockId, _flagAir);
                    }
                }
            }
        }
    }
}
