using Mvk2.World.Block;
using Vge.World.Element;

namespace Mvk2.World.Element
{
    /// <summary>
    /// Объект генерации блоков MVK, для элементов, структур и прочего, не генерации чанка
    /// Для роста деревьев и прочего, где взаимодействие множества блоков.
    /// Т.е. когда уже мир сгенерирован, и делаем доп генерации
    /// </summary>
    public class BlocksElementGeneratorMvk : BlocksElementGenerator
    {
        public BlocksElementGeneratorMvk()
        {
            _elements = new IElementGenerator[2];
            _elements[(int)EnumElementGen.TreeBirch] = new ElementGenTree(BlockCaches, 
                BlocksRegMvk.LogBirch.IndexBlock, BlocksRegMvk.BranchBirch.IndexBlock, 
                BlocksRegMvk.LeavesBirch.IndexBlock, BlocksRegMvk.SaplingBirch.IndexBlock);
            _elements[(int)EnumElementGen.TreeOak] = new ElementGenTree(BlockCaches,
                BlocksRegMvk.LogOak.IndexBlock, BlocksRegMvk.BranchOak.IndexBlock,
                BlocksRegMvk.LeavesOak.IndexBlock, BlocksRegMvk.SaplingOak.IndexBlock);
        }
    }
}
