namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок плода дуба
    /// </summary>
    public class BlockFetusOak : BlockFetus
    {
        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов и корректировки id блоков
        /// </summary>
        public override void InitAfterItems()
            => _idLeaves = BlocksRegMvk.LeavesOak.IndexBlock;
    }
}
