using Mvk2.World.Block;
using Vge.World.Element;

namespace Mvk2.World.Element
{
    /// <summary>
    /// Объект изменения блоков в MVK, для элементов, структур и прочего, в обновлении мира.
    /// Для роста деревьев и прочего, где взаимодействие множества блоков.
    /// </summary>
    public class BlocksElementUpdateMvk : BlocksElementUpdate
    {
        public BlocksElementUpdateMvk()
        {
            _elements = new IElementUpdate[2];
            _elements[(int)EnumElementUpdate.TreeBirch] = new ElementGrowthTree(BlockCaches, 
                BlocksRegMvk.LogBirch.IndexBlock, BlocksRegMvk.BranchBirch.IndexBlock, 
                BlocksRegMvk.LeavesBirch.IndexBlock, BlocksRegMvk.SaplingBirch.IndexBlock);
            _elements[(int)EnumElementUpdate.TreeOak] = new ElementGrowthTree(BlockCaches,
                BlocksRegMvk.LogOak.IndexBlock, BlocksRegMvk.BranchOak.IndexBlock,
                BlocksRegMvk.LeavesOak.IndexBlock, BlocksRegMvk.SaplingOak.IndexBlock);
        }
    }
}
