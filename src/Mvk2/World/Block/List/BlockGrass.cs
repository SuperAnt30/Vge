using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок травы
    /// </summary>
    public class BlockGrass : BlockBase
    {
        protected bool _biomeColor;

        public BlockGrass(MaterialBase material) : base(material) { }

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb)
        {
            int i = (xb + zb) & 7;
            if (i > 4) i -= 4;
            return _quads[i];
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, BlockBase neighborBlock)
        {
            base.NeighborBlockChange(world, chunk,blockPos, blockState, neighborBlock);
            if (!CanBlockStay(world, chunk, blockPos))
            {
                //DropBlockAsItem(worldIn, blockPos, state);
                world.SetBlockToAir(blockPos);
            }
        }

        /// <summary> 
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase world, ChunkBase chunk, BlockPos blockPos, int met = 0)
        {
            if (chunk == null) chunk = world.GetChunk(blockPos);
            if (chunk != null)
            {
                BlockBase block = chunk.GetBlockState(blockPos.OffsetDown()).GetBlock();
                return block.IndexBlock == BlocksRegMvk.TurfLoam.IndexBlock;
            }
            return false;
        }
    }
}
