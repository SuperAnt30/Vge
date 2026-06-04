using Vge.World.Block;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок листвы плодовых
    /// </summary>
    public class BlockLeavesFruit : BlockLeaves
    {
        public BlockLeavesFruit(MaterialBase material) : base(material) { }

        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов и корректировки id блоков
        /// </summary>
        public override void InitAfterItems()
        {
            _idLog = BlocksRegMvk.LogFruit.IndexBlock;
            _idBranch = BlocksRegMvk.BranchFruit.IndexBlock;
            _idFetus = BlocksRegMvk.FetusFruit.IndexBlock;
        }
    }
}
