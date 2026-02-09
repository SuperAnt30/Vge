using Mvk2.World.BlockEntity.List;
using System.Runtime.CompilerServices;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок древесного корня
    /// </summary>
    public class BlockRoot : BlockBase
    {
        /***
         * Met
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 
         * 
         */

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met & 0xF];

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnBreakBlock(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState stateOld, BlockState stateNew)
        {

            int y = blockPos.Y + 1;
            int xz = (blockPos.Z & 15) << 4 | (blockPos.X & 15);
            while (chunk.GetBlockStateNotCheckLight(xz, y).Id == IndexBlock)
            {
                y++;
            }
            blockPos.Y = y + 1;
            if (chunk.GetBlockEntity(blockPos) is BlockEntityTree blockEntityTree)
            {
                blockEntityTree.RemoveAllLeaves(world);
            }

           // chunk.AddRangeBlockEntity(blocksEntity);

            //if (Type == TypeTree.Log || Type == TypeTree.Branch)
            //{



            //    // Список всех блок сущностей в квадрате 3*3 чанка
            //    List<BlockEntityBase> blocksEntity = world.GetBlocksEntity3x3(chunk);

            //    // Пробегаемся и производим удаление
            //    foreach (BlockEntityBase blockEntity in blocksEntity)
            //    {
            //        if (blockEntity is BlockEntityTree blockEntityTree
            //            && blockEntityTree.IsAABB(blockPos))
            //        {
            //            // Это блок возможно принадлежит этому дереву. Откусить
            //            blockEntityTree.RemoveBlock(world, chunk, blockPos);
            //        }
            //    }
            //}
        }
    }
}
