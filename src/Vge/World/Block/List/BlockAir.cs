using System.Runtime.CompilerServices;

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
        public BlockAir() => _SetAir();

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsPassable(uint met) => true;

        /// <summary>
        /// Имеется ли отбраковка конкретной стороны, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsCullFace(int met, int indexSide) => false;
        /// <summary>
        /// Надо ли принудительно рисовать сторону, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsForceDrawFace(int met, int indexSide) => false;
        /// <summary>
        /// Надо ли принудительно рисовать не крайнюю сторону, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsForceDrawNotExtremeFace(int met, int indexSide) => false;
        /// <summary>
        /// Проверка масок сторон
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ChekMaskCullFace(int indexSide, int met, BlockBase blockSide, int metSide) => false;

        /// <summary>
        /// Может ли быть дополнительная жидкость в этом блоке
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanAddLiquid() => false;
    }
}
