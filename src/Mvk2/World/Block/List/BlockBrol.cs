using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vge.Entity.Player;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
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
    }
}
