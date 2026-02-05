using System.Runtime.CompilerServices;
using Vge.World;
using Vge.World.Block;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок листвы
    /// </summary>
    public class BlockLeaves : BlockTree
    {
        /// <summary>
        /// ID блок бревна текущего дерева
        /// </summary>
        protected int _idLog;
        /// <summary>
        /// ID блок ветки текущего дерева
        /// </summary>
        protected int _idBranch;

        public BlockLeaves() : base(TypeTree.Leaves) { }

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb)
        {
            return _quads[met & 0xFF];
        }

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
            if (met > 5) met -= 6;
            int id = world.GetBlockState(blockPos.OffsetReversal(met)).Id;
            return id == _idLog || id == _idBranch;
        }
    }
}
