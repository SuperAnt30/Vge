namespace Vge.World.Block.List
{
    /// <summary>
    /// Отсутствующий блок, может появится если в новой версии не будет прошлого блока
    /// </summary>
    public class BlockNull : BlockBase
    {
        /// <summary>
        /// Отсутствующий блок, может появится если в новой версии не будет прошлого блока
        /// </summary>
        public BlockNull() => SetAir();

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(uint met) => true;
    }
}
