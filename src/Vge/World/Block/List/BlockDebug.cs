using Vge.Util;
using WinGL.Util;

namespace Vge.World.Block.List
{
    /// <summary>
    /// Универсальный объект твёрдых блоков
    /// </summary>
    public class BlockDebug : BlockBase
    {
        public BlockDebug()
        {
            //Particle = numberTexture;
            //_InitQuads(numberTexture);
        }

        /// <summary>
        /// Стороны целого блока для рендера
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _GetQuads(met & 0xFF);

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state,
            Pole side, Vector3 facing)
        {
            // Определяем на какую сторону смотрит игрок
            if (side == Pole.East) return state.NewMet(2);
            if (side == Pole.South) return state.NewMet(1);
            if (side == Pole.North) return state.NewMet(3);
            return state.NewMet(0);
        }

    }
}
