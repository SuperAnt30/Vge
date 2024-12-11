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

        /// <summary>
        /// Имеется ли отбраковка конкретной стороны, конкретного варианта
        /// </summary>
        public override bool IsCullFace(uint met, int indexSide) => false;
        /// <summary>
        /// Надо ли принудительно рисовать сторону, конкретного варианта
        /// </summary>
        public override bool IsForceDrawFace(uint met, int indexSide) => false;
        /// <summary>
        /// Надо ли принудительно рисовать не крайнюю сторону, конкретного варианта
        /// </summary>
        public override bool IsForceDrawNotExtremeFace(uint met, int indexSide) => false;
        /// <summary>
        /// Проверка масок сторон
        /// </summary>
        public override bool ChekMaskCullFace(int indexSide, uint met, BlockBase blockSide, uint metSide) => false;
    }
}
