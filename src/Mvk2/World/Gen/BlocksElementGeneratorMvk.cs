using Mvk2.World.Block;
using Vge.World.Gen;
using Vge.World.Gen.Element;

namespace Mvk2.World.Gen
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
            _elements[(int)EnumElement.TreeBirch] = new ElementTree(BlockCaches, 
                BlocksRegMvk.LogBirch.IndexBlock, BlocksRegMvk.BranchBirch.IndexBlock, 
                BlocksRegMvk.LeavesBirch.IndexBlock);
            _elements[(int)EnumElement.TreeOak] = new ElementTree(BlockCaches,
                BlocksRegMvk.LogOak.IndexBlock, BlocksRegMvk.BranchOak.IndexBlock,
                BlocksRegMvk.LeavesOak.IndexBlock);
        }
    }
}
