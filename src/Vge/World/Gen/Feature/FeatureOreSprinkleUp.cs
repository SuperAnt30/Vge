using Vge.Util;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.World.Gen.Feature
{
    /// <summary>
    /// Генерация разброса руды сверху
    /// </summary>
    public class FeatureOreSprinkleUp : FeatureColumn
    {
        private readonly ushort _blockWhereId;
        /// <summary>
        /// Толщина насыпи минимальная
        /// </summary>
        private readonly byte _depthMin;
        /// <summary>
        /// Толщина насыпи случайная, дополнительно к минимальной
        /// </summary>
        private readonly byte _depthRandom;

        public FeatureOreSprinkleUp(IChunkPrimer chunkPrimer, byte minRandom, byte maxRandom, 
           byte depthMin, byte depthRandom, ushort blockId, ushort blockWhereId)
           : base(chunkPrimer, minRandom, maxRandom, blockId)
        {
            _depthMin = depthMin;
            _depthRandom = depthRandom;
            _blockWhereId = blockWhereId;
        }

        public FeatureOreSprinkleUp(IChunkPrimer chunkPrimer, byte probabilityOne, 
            byte depthMin, byte depthRandom, ushort blockId, ushort blockWhereId)
            : base (chunkPrimer, probabilityOne, blockId)
        {
            _depthMin = depthMin;
            _depthRandom = depthRandom;
            _blockWhereId = blockWhereId;
        }

        /// <summary>
        /// Декорация блока или столба не выходящего за чанк, одного прохода
        /// </summary>
        protected override void _DecorationColumnOctave(ChunkBase chunkSpawn, Rand rand)
        {
            int depth = _depthMin + rand.Next(_depthRandom);
            int z0 = rand.Next(14) + 1;
            int x0 = rand.Next(14) + 1;
            int xz = z0 << 4 | x0;
                
            int y = chunkSpawn.HeightMapGen[xz] - rand.Next(9) + 4;
            if (y > chunkSpawn.Settings.NumberMaxBlock)
            {
                y = chunkSpawn.Settings.NumberMaxBlock;
            }

            int i, x, z, z1;
            
            for (i = 0; i < depth; i++)
            {
                if (y > 0)
                {
                    for (z = -1; z < 2; z++)
                    {
                        z1 = (z0 + z) << 4;
                        for (x = -1; x < 2; x++)
                        {
                            if (rand.Next((Mth.Abs(x) + Mth.Abs(z)) * 2 + 2) == 0)
                            {
                                xz = z1 | (x0 + x);
                                if (_chunkPrimer.GetBlockId(xz, y) == _blockWhereId)
                                {
                                    _chunkPrimer.SetBlockState(xz, y, _blockId);
                                }
                            }
                        }
                    }
                }
                if (y < 4) break;
                y--;
            }
        }
    }
}
