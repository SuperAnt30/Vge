namespace Vge.World.Block.List
{
    /// <summary>
    /// Блок воздуха, пустота
    /// </summary>
    public class BlockAir : BlockBase
    {
        /// <summary>
        /// Блок воздуха
        /// </summary>
        public BlockAir() => SetAir();

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public override bool IsPassable(uint met) => true;
    }
}
