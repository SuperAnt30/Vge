using Vge.World.Block;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок плода дуба
    /// </summary>
    public class BlockFetusOak : BlockFetus
    {
        public BlockFetusOak(IMaterial material) : base(material) { }

        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов и корректировки id блоков
        /// </summary>
        public override void InitAfterItems()
            => _idLeaves = BlocksRegMvk.LeavesOak.IndexBlock;
    }
}
