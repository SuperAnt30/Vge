using System.Runtime.CompilerServices;

namespace Vge.World.Block.List
{
    /// <summary>
    /// Блок ветки
    /// </summary>
    public class BlockBranch : BlockBase
    {
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
