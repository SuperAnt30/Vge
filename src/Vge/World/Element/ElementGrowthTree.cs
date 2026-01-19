using Vge.Util;
using Vge.World.Block;

namespace Vge.World.Element
{
    /// <summary>
    /// Объект роста или старения дерева, в такте мира
    /// </summary>
    public class ElementGrowthTree : IElementUpdate
    {
        /// <summary>
        /// Индекс блока бревна
        /// </summary>
        private readonly int _blockLogId;
        /// <summary>
        /// Индекс блока ветки
        /// </summary>
        private readonly int _blockBranchId;
        /// <summary>
        /// Индекс блока листвы
        /// </summary>
        private readonly int _blockLeavesId;
        /// <summary>
        /// Индекс блока саженца
        /// </summary>
        private readonly int _blockSaplingId;

        /// <summary>
        /// Список блоков которые будет построено дерево
        /// </summary>
        protected ArrayFast<BlockCache> _blockCaches;

        public ElementGrowthTree(ArrayFast<BlockCache> blockCache, int blockLogId, int blockBranchId, 
            int blockLeavesId, int blockSaplingId)
        {
            _blockCaches = blockCache;
            _blockLogId = blockLogId;
            _blockBranchId = blockBranchId;
            _blockLeavesId = blockLeavesId;
            _blockSaplingId = blockSaplingId;
        }

        /// <summary>
        /// Изменение элемента в выбранной позиции
        /// </summary>
        public void Update(WorldServer world, BlockPos blockPos)
        {
            _blockCaches.Clear();

            // TODO::2026-01-19 тут надо продумать заход в место где-то в чанке сервера данные деревьев.
            // И по ним по маске делать анализ, как ростём.
            // Так же для этой маски нужен алгоритм откусывания веток
            IElementMask elementMask = world.GetElementMask(blockPos);

            if (elementMask != null)
            {
                // Уже имеется маска дерева, анализируем рост

            }
            else
            {
                // Маски нет, скорее всего это саженец ростёт
                BlockState blockState = world.GetBlockState(blockPos);
                //BlockBase block = blockState.GetBlock();
                if (blockState.Id == _blockSaplingId)
                {
                    // Проверяем саженец ли это

                }
            }

            _blockCaches.Add(new BlockCache(blockPos, _blockLogId));
            _blockCaches.Add(new BlockCache(blockPos.OffsetUp(), _blockBranchId));
            _blockCaches.Add(new BlockCache(blockPos.OffsetUp(2), _blockLeavesId));
            //world.ChunkPrServ.ChunkGenerate.
            //world.SetBlockState(blockPos, new BlockState(_blockBranchId), 46);
            //world.SetBlockState(blockPos.OffsetUp(), new BlockState(_blockLeavesId), 46);
            world.ExportBlockCaches();
        }

    }
}
