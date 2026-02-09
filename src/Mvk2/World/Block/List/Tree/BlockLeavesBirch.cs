using Vge.World.Block;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок листвы берёзы
    /// </summary>
    public class BlockLeavesBirch : BlockLeaves
    {
        public BlockLeavesBirch(IMaterial material) : base(material) { }

        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов и корректировки id блоков
        /// </summary>
        public override void InitAfterItems()
        {
            _idLog = BlocksRegMvk.LogBirch.IndexBlock;
            _idBranch = BlocksRegMvk.BranchBirch.IndexBlock;
            _idFetus = BlocksRegMvk.FetusBirch.IndexBlock;
        }
    }
}
