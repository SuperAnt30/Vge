using System.Runtime.CompilerServices;
using Vge.Util;

namespace Vge.World.Block.List
{
    /// <summary>
    /// Блок бревна
    /// </summary>
    public class BlockLog : BlockBase
    {
        /***
         * Met
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 4 - вверх, генерация, нижний блок для пня и тика
         * 5 - вверх, игрок
         * 6/7 - бок, игрок
         */

        /// <summary>
        /// Индекс элемента для генерации
        /// </summary>
        private readonly int _elementId;

        public BlockLog(int elementId) => _elementId = elementId;

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met];

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        //public override void UpdateTick(WorldServer world, BlockPos blockPos, BlockState blockState, Rand random)
        //    => world.Settings.BlocksElement.Element(_elementId)?.Update(world, blockPos);
    }
}
