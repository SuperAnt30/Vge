using Vge.Util;
using Vge.World.Block;

namespace Vge.World.Gen.Element
{
    /// <summary>
    /// Объект генерации элемента ромта дерева
    /// </summary>
    public class ElementTree : IElementGenerator
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
        /// Список блоков которые будет построено дерево
        /// </summary>
        protected ArrayFast<BlockCache> _blockCaches;

        public ElementTree(ArrayFast<BlockCache> blockCache, int blockLogId, int blockBranchId, int blockLeavesId)
        {
            _blockCaches = blockCache;
            _blockLogId = blockLogId;
            _blockBranchId = blockBranchId;
            _blockLeavesId = blockLeavesId;
        }

        /// <summary>
        /// Генерация элемента в выбранной позиции
        /// </summary>
        public void Generation(WorldServer world, BlockPos blockPos)
        {
            _blockCaches.Clear();

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
