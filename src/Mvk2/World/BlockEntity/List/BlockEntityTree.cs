using Vge.World.Block;
using Vge.World.BlockEntity;

namespace Mvk2.World.BlockEntity.List
{
    /// <summary>
    /// Блок сущности дерева
    /// </summary>
    public class BlockEntityTree : BlockEntityBase
    {
        private BlockCache[] _blockCaches;

        public void SetArray(BlockCache[] blockCaches)
        {
            _blockCaches = blockCaches;
        }

        public int Count() => _blockCaches.Length;

        public BlockPos GetBlockPos(int index) => _blockCaches[index].Position;

        public int GetBlockId(int index) => _blockCaches[index].Id;
    }
}
