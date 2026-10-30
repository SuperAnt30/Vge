using System.Runtime.CompilerServices;
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
            _fullCube = new AxisAlignedBB[][] { 
                new AxisAlignedBB[] 
                { 
                    new AxisAlignedBB(0, .5f, 0, .5f, 1, 1),
                    new AxisAlignedBB(0, 0, 0, 1, .5f, 1)
                },
                new AxisAlignedBB[]
                {
                    new AxisAlignedBB(0, .5f, .5f, 1, 1, 1),
                    new AxisAlignedBB(0, 0, 0, 1, .5f, 1)
                },
                new AxisAlignedBB[]
                {
                    new AxisAlignedBB(.5f, .5f, 0, 1, 1, 1),
                    new AxisAlignedBB(0, 0, 0, 1, .5f, 1)
                },
                new AxisAlignedBB[]
                {
                    new AxisAlignedBB(0, .5f, 0, 1, 1, .5f),
                    new AxisAlignedBB(0, 0, 0, 1, .5f, 1)
                }
            };
        }

        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override AxisAlignedBB[] _GetCollisionBoxesLocal(int met) => _fullCube[met & 3];

        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector3[] _GetOutlineLocal(int met) => _hitbox[met & 3];

        /// <summary>
        /// Стороны целого блока для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _GetQuads(met & 0x3F);

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
    }
}
