using Mvk2.World.BlockEntity.List;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.BlockEntity;
using Vge.World.Chunk;
using Vge.World.Сalendar;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок листвы
    /// </summary>
    public class BlockLeaves : BlockTree
    {
        /***
         * Met 0001 0000 1111
         * Для Leaves листва 4 bit форма 1 bit молодой?
         * 0 - вверх
         * 1 - низ
         * 2-5 бок
         * 6 - вверх 2
         * 7 - низ 2
         * 8-11 бок 2
         * 
         * +256 - молодой, нельзя выращивать плоды || Можно вешать плод
         * +512 - надо удалять
         */

        /// <summary>
        /// ID блок бревна текущего дерева
        /// </summary>
        protected int _idLog;
        /// <summary>
        /// ID блок ветки текущего дерева
        /// </summary>
        protected int _idBranch;
        /// <summary>
        /// ID блок плода текущего дерева
        /// </summary>
        protected int _idFetus;

        public BlockLeaves() : base(TypeTree.Leaves) { }

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
            if (met > 255) met = met & 0xF;
            if (met > 5) met -= 6;
            int id = world.GetBlockState(blockPos.OffsetReversal(met)).Id;
            return id == _idLog || id == _idBranch;
        }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        public override void RandomTick(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, Rand random)
        {
            int met = blockState.Met;
         //   if (met < 255)// && chunk.GetBlockStateNotCheck(blockPos.OffsetDown()).Id == 0)
            {
                // Найти ветку
                if (met > 255) met = met & 0xF;
                if (met > 5) met -= 6;

                BlockPos blockPosBranch = blockPos.OffsetReversal(met);

                // Список всех блок сущностей в квадрате 3*3 чанка
                List<BlockEntityBase> blocksEntity = world.GetBlocksEntity3x3(chunk);

                // Пробегаемся и производим удаление
                foreach (BlockEntityBase blockEntity in blocksEntity)
                {
                    if (blockEntity is BlockEntityTree blockEntityTree
                        && blockEntityTree.IsAABB(blockPosBranch)
                        && blockEntityTree.FindBlock(blockPosBranch))
                    {

                        if (blockEntityTree.Step == BlockEntityTree.TypeStep.Dry)
                        {
                            world.SetBlockToAir(blockPos);
                        }
                        else if (blockEntityTree.Step == BlockEntityTree.TypeStep.Norm)
                        {
                            // Тут мы бываем только при TypeStep.Norm
                            // Проверяем состояние дерева черз тик основания
                            blockEntityTree.CheckingCondition();

                            //if (world.Settings.Calendar is Сalendar32 сalendar32 
                            //    && сalendar32.TimeYear == EnumTimeYear.Spring)
                            if (chunk.GetBlockStateNotCheck(blockPos.OffsetDown()).Id == 0
                                && blockEntityTree.IsAddFetus())
                            {
                                chunk.SetBlockState(blockPos.OffsetDown(), new BlockState(_idFetus), 12);
                            }
                        }
                        // Делаем обновление тика листвы
                        // blockEntityTree.UpdateTickLeaves(world, chunk, blockPosBranch, blockPos, _idFetus);
                    // Это блок возможно принадлежит этому дереву. Откусить
                        //blockEntityTree.RemoveBlock(world, chunk, blockPosBranch);
                    }
                }


               // world.SetBlockState(blockPos.OffsetDown(), new BlockState(_idFetus), 12);
            }
        }
    }
}
