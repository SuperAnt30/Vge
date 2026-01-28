using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Block;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок ветки
    /// </summary>
    public class BlockBranch : BlockLog
    {

        public BlockBranch(int elementId) : base(elementId) { }

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met & 0xFF];

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        //public override void UpdateTick(WorldServer world, BlockPos blockPos, BlockState blockState, Rand random)
        //    => world.Settings.BlocksElement.Element(_elementId)?.Update(world, blockPos);
    }
}
