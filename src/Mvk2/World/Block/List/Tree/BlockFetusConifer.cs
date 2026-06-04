using Vge.World.Block;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок плода хвойных
    /// </summary>
    public class BlockFetusConifer : BlockFetus
    {
        public BlockFetusConifer(MaterialBase material) : base(material) { }

        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов и корректировки id блоков
        /// </summary>
        public override void InitAfterItems()
            => _idLeaves = BlocksRegMvk.LeavesConifer.IndexBlock;
    }
}
