using System.Runtime.CompilerServices;
using Vge.World.Block;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок высокой травы
    /// </summary>
    public class BlockTallGrass : BlockBase
    {
        /***
        * Met
        * 0 - низ
        * 1 - вверх
        */

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb)
        {
            int i = (xb + zb) & 7;
            if (i > 4) i -= 4;
            if (met == 1) i += 5;
            return _quads[i];
        }
    }
}
