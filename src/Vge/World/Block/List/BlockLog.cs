using System.Runtime.CompilerServices;

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
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb)
        {
            return _quads[met];
        }
    }
}
