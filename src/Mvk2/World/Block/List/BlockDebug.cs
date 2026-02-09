using Vge.Util;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Универсальный объект твёрдых блоков
    /// </summary>
    public class BlockDebug : BlockBase
    {
        public BlockDebug(IMaterial material) : base(material)
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
        public override BlockState OnBlockPlaced(WorldServer worldIn, BlockPos blockPos, BlockState state,
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
