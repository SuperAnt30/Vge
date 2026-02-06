using System.Runtime.CompilerServices;
using Vge.World;
using Vge.World.Block;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок плода
    /// </summary>
    public class BlockFetus : BlockBase
    {
        /// <summary>
        /// ID блок листвы текущего дерева
        /// </summary>
        protected int _idLeaves;

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldServer world, BlockPos blockPos,
            BlockState blockState, BlockBase neighborBlock)
        {
            base.NeighborBlockChange(world, blockPos, blockState, neighborBlock);
            if (!CanBlockStay(world, blockPos, blockState.Met))
            {
                //DropBlockAsItem(worldIn, blockPos, state);
                world.SetBlockToAir(blockPos, 47); // 1 2 4 8 32 без звука но с частичками
            }
        }

        /// <summary> 
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanBlockStay(WorldServer world, BlockPos blockPos, int met = 0)
        {
            return world.GetBlockState(blockPos.OffsetUp()).Id == _idLeaves;
        }
    }
}
