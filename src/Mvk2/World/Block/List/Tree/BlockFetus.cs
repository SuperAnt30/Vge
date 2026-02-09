using System.Runtime.CompilerServices;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок плода
    /// </summary>
    public class BlockFetus : BlockBase
    {
        /***
         * Met
         * 0 - Зелёное
         * 1 - Жёлтое
         * 2 - Красное
         * 3 - Гнилое
         */

        /// <summary>
        /// ID блок листвы текущего дерева
        /// </summary>
        protected int _idLeaves;

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, BlockBase neighborBlock)
        {
            base.NeighborBlockChange(world, chunk, blockPos, blockState, neighborBlock);
            if (!CanBlockStay(world, chunk, blockPos, blockState.Met))
            {
                //DropBlockAsItem(worldIn, blockPos, state);
                world.SetBlockToAir(blockPos, 47); // 1 2 4 8 32 без звука но с частичками
            }
        }

        /// <summary> 
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanBlockStay(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, int met = 0)
        {
            return chunk.GetBlockState(blockPos.OffsetUp()).Id == _idLeaves;
        }
    }
}
