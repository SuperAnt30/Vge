using Vge.Util;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.World.Gen.Feature
{
    /// <summary>
    /// Генерация особенностей, валун
    /// </summary>
    public class FeatureOreSprinkle : IFeatureGeneratorColumn
    {
        protected IChunkPrimer _chunkPrimer;

        /// <summary>
        /// Какой блок ставим
        /// </summary>
        private readonly ushort _blockId;

        private readonly ushort _blockWhereId;
        /// <summary>
        /// Вероятность одной
        /// </summary>
        private byte _probabilityOne;
        /// <summary>
        /// Удача количеств в ячейке, 1 = 100%, чем больше тем меньше шанс
        /// </summary>
        private byte _fortune;
        /// <summary>
        /// Толщина насыпи
        /// </summary>
        private byte _depth;

        public FeatureOreSprinkle(IChunkPrimer chunkPrimer, byte probabilityOne, byte fortune,
            byte depth, ushort blockId, ushort blockWhereId)
        {
            _chunkPrimer = chunkPrimer;
            _probabilityOne = probabilityOne;
            _fortune = fortune;
            _depth = depth;
            _blockId = blockId;
            _blockWhereId = blockWhereId;
        }

        /// <summary>
        /// Декорация блока или столба не выходящего за чанк
        /// </summary>
        public void DecorationsColumn(ChunkBase chunkSpawn, Rand rand)
        {
            if (rand.Next(_probabilityOne) == 0)
            {
                int z0 = rand.Next(14) + 1;
                int x0 = rand.Next(14) + 1;
                int xz = z0 << 4 | x0;
                
                int y = chunkSpawn.HeightMapGen[xz] - rand.Next(5) + 2; // Может выйти за пределы высот
                y += 10;
                //chunkSpawn.NumberSections
                int x, z;
                int x1 = x0 - 1;
                int x2 = x0 + 1;
                int z1 = z0 - 1;
                int z2 = z0 + 1;

                for (int i = 0; i < _depth; i++)
                {
                    if (y > 0)
                    {
                        for (z = z1; z <= z2; z++)
                        {
                          //  z0 = z << 4;
                            for (x = x1; x <= x2; x++)
                            {
                                
                                _fortune = (byte)((Mth.Abs(x0 - x) + Mth.Abs(z0 - z)) * 2 + 2);
                               // if (_fortune == 3) _fortune = 5;
                                //_fortune = 1;
                                if (_fortune == 1 || rand.Next(_fortune) == 0)
                                {
                                    xz = z << 4 | x;
                                 //   if (_chunkPrimer.GetBlockId(xz, y) == _blockWhereId)
                                    {
                                        _chunkPrimer.SetBlockState(xz, y, _blockId);
                                    }
                                }
                            }
                        }
                        
                        //BlockState blockState = chunkSpawn.GetBlockStateNotCheckLight(xz, y - 1);
                        //if (blockState.Id == BlocksRegMvk.Granite.IndexBlock)
                        //{
                        //    for (int y2 = 0; y2 < cloum; y2++)
                        //    {
                        //if (_chunkPrimer.GetBlockId(xz, y) == _blockWhereId)
                        //{
                        //    _chunkPrimer.SetBlockState(xz, y, _blockId);
                        //}
                        //    }
                        //}
                    }
                    y--;
                }
            }
        }
    }
}
