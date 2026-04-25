using Mvk2.Particle;
using Vge.Entity.Player;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Mvk2.World.Block.List
{
    public class BlockBrol : BlockBase
    {
        public BlockBrol(MaterialBase material) : base(material) { }

        /// <summary>
        /// Активация блока, true - был клик, false - нет такой возможности
        /// </summary>
        /// <param name="pos">Позиция блока, по которому щелкают</param>
        /// <param name="side">Сторона, по которой щелкнули</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool OnBlockActivated(PlayerBase player, BlockPos blockPos, Pole side, Vector3 facing)
        {
            WorldBase world = player.GetWorld();
            if (!world.IsRemote)
            {
                if (side == Pole.Up)
                {
                    if (world.GetBlockState(blockPos.OffsetUp()).GetBlock().IsAir)
                    {
                        world.SetBlockState(blockPos.OffsetUp(), world.GetBlockState(blockPos), 46);
                        world.SetBlockToAir(blockPos);
                    }
                }
                else
                {
                    if (world.GetBlockState(blockPos.OffsetDown()).GetBlock().IsAir)
                    {
                        world.SetBlockState(blockPos.OffsetDown(), world.GetBlockState(blockPos), 46);
                        world.SetBlockToAir(blockPos);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldClient world, BlockPos blockPos, BlockState blockState)
        {
            Vector3 pos = blockPos.ToVector3Center();
            pos.Y += .75f;
            //world.SpawnParticle(1, 1, pos, new Vector3(.5f), 1, 0);
            //world.SpawnParticle((ushort)EnumParticles.Smoke, 5, pos, new Vector3(.5f), 1, 0);
        }

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RequiredRandomDisplayTick(WorldClient world, BlockPos blockPos, BlockState blockState)
        {
            Vector3 pos = blockPos.ToVector3Center();
            pos.Y += .75f;
            world.SpawnParticle(EntitiesFXRegMvk.SmokeId, 1, pos, new Vector3(.5f), 1, 0);
        }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        public override void RandomTick(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, Rand random)
        {
            //Vector3 pos = blockPos.ToVector3Center();
            //pos.Y += .75f;
            //world.SpawnParticle((ushort)EnumParticles.Smoke, 5, pos, new Vector3(.5f), 1, 0);
        }
    }
}
