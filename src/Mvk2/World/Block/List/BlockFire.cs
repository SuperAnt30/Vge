using Mvk2.Entity.Particle;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок огня
    /// </summary>
    public class BlockFire : BlockBase
    {
        public BlockFire(MaterialBase material) : base(material)
        {
            //IsAction = false;

            _fullCube = new AxisAlignedBB[][] {
                new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 0, 16, 16, 16) }, // 0
                //new AxisAlignedBB[] { new AxisAlignedBB(0, .25f, .25f, 1, .75f, .75f) }, // 1
                //new AxisAlignedBB[] { new AxisAlignedBB(.25f, .25f, 0, .75f, .75f, 1) }, // 2
                //new AxisAlignedBB[] { new AxisAlignedBB(.5f, 0, .25f, 1, 1, .75f) }, // 3
                //new AxisAlignedBB[] { new AxisAlignedBB(0, 0, .25f, .5f, 1, .75f) }, // 4
                //new AxisAlignedBB[] { new AxisAlignedBB(.25f, 0, 0, .75f, 1, .5f) }, // 5
                //new AxisAlignedBB[] { new AxisAlignedBB(.25f, 0, .5f, .75f, 1, 1) } // 6
            };
            _InitFullCubeDivide16();
        }

        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        public override AxisAlignedBB[] _GetCollisionBoxesLocal(int met) => _fullCube[met & 7];
        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        public override Vector3[] _GetOutlineLocal(int met) => _hitbox[met & 7];

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldClient world, BlockPos blockPos, BlockState blockState)
        {
            world.SpawnParticle(EntitiesFXRegMvk.FlameSmokeId, 16,
                blockPos.ToVector3Center(), new Vector3(1, 0, 1), 1, 0);
        }

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RequiredRandomDisplayTick(WorldClient world, BlockPos blockPos, BlockState blockState)
        {
            if (world.Rnd.Next(3) == 0)
            {
                world.SpawnParticle(EntitiesFXRegMvk.FlameSmokeId, 4,
                    blockPos.ToVector3Center(), new Vector3(1, 0, 1), 1, 0);
            }
        }
    }
}
