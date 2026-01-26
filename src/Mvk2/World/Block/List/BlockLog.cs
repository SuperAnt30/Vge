using Mvk2.World.BlockEntity.List;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок бревна
    /// </summary>
    public class BlockLog : BlockBase
    {
        /***
         * Met
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 3 - вверх, генерация, нижний блок для пня и тика
         * 4 - вверх, игрок
         * 5/6 - бок, игрок
         */

        /// <summary>
        /// Индекс элемента для генерации
        /// </summary>
        private readonly int _elementId;

        public BlockLog(int elementId) => _elementId = elementId;

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met];

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (blockState.Met == 3)
            {
                // Это пень и мы должны проверить наличие блок сущности на сервере
                BlockEntityTree blockEntityTree = chunk.GetBlockEntity(blockPos) as BlockEntityTree;
                if (blockEntityTree != null)
                {
                    int count = blockEntityTree.Count();
                    for (int i = 0; i < count; i++)
                    {
                        if (blockEntityTree.GetBlockId(i) != IndexBlock)
                        {
                            BlockPos pos = blockEntityTree.GetBlockPos(i);
                            pos.X += chunk.BlockX;
                            pos.Z += chunk.BlockZ;
                            world.SetBlockToAir(pos);
                        }
                    }
                }
                world.SetBlockStateMet(blockPos, 0);
            }
        }
        //    => world.Settings.BlocksElement.Element(_elementId)?.Update(world, blockPos);
    }
}
