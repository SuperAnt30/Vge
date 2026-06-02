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
        public BlockDebug(MaterialBase material) : base(material)
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
            Pole side, Vector3 facing, float playerYaw)
        {
            // Определяем на какую сторону смотрит игрок
            if (side == Pole.Up)
            {
                // Стороны по playerYaw
                side = (Pole)PoleConvert.Reverse[(int)PoleConvert.FromAngle(playerYaw)];
            }
            
            if (side == Pole.East) return state.NewMet(0);
            if (side == Pole.South) return state.NewMet(3);
            if (side == Pole.North) return state.NewMet(1);
            return state.NewMet(2);
        }

        /// <summary>
        /// Передать список  ограничительных рамок блока
        /// </summary>
      //  [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            met = met & 3;

            if (met == 1) return new AxisAlignedBB[] {
                new AxisAlignedBB(pos.X, pos.Y + .5f, pos.Z + .5f, pos.X + 1, pos.Y + 1, pos.Z + 1),
                new AxisAlignedBB(pos.X , pos.Y, pos.Z, pos.X + 1, pos.Y + .5f, pos.Z + 1)
            };
            if (met == 2) return new AxisAlignedBB[] {
                new AxisAlignedBB(pos.X + .5f, pos.Y + .5f, pos.Z, pos.X + 1, pos.Y + 1, pos.Z + 1),
                new AxisAlignedBB(pos.X , pos.Y, pos.Z, pos.X + 1, pos.Y + .5f, pos.Z + 1)
            };
            if (met == 3) return new AxisAlignedBB[] {
                new AxisAlignedBB(pos.X, pos.Y + .5f, pos.Z, pos.X + 1, pos.Y + 1, pos.Z + .5f),
                new AxisAlignedBB(pos.X , pos.Y, pos.Z, pos.X + 1, pos.Y + .5f, pos.Z + 1)
            };
            return new AxisAlignedBB[] { 
                new AxisAlignedBB(pos.X , pos.Y + .5f, pos.Z, pos.X + .5f, pos.Y + 1, pos.Z + 1),
                new AxisAlignedBB(pos.X , pos.Y, pos.Z, pos.X + 1, pos.Y + .5f, pos.Z + 1)
            };
        }
    }
}
