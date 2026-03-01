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

namespace Vge.Item.List
{
    /// <summary>
    /// Предмет который ставится как блок
    /// </summary>
    public class ItemBlock : ItemBase
    {
        /// <summary>
        /// Объект блока который устанавливается этим предметом
        /// </summary>
        public readonly BlockBase Block;

        public ItemBlock(BlockBase block) => Block = block;

        #region Дейстыия рук, ЛКМ и ПКМ

        /// <summary>
        /// Вызывается, когда блок щелкают ПКМ с этим элементом, возвращает true если действие состоялось
        /// </summary>
        /// <param name="pos">Позиция блока, по которому щелкают ПКМ</param>
        /// <param name="side">Сторона, по которой щелкнули ПКМ</param>
        /// <param name="facing"></param>
        public override bool OnItemOnBlockUseSecond(ItemStack stack, PlayerBase player,
            BlockPos blockPos, Pole side, Vector3 facing)
        {
            WorldBase world = player.GetWorld();

            if (!world.GetBlockState(blockPos).GetBlock().IsReplaceable)
            {
                blockPos = blockPos.Offset(side);
            }

            if (_CanPlaceBlockOnSide(stack, player, world, blockPos, Block, side, facing))
            {
                BlockState blockState = Block.OnBlockPlaced(world, blockPos, 
                    new BlockState(Block.IndexBlock), side, facing);
                if (Block.CanBlockStay(world, blockPos, blockState.Met))
                {
                    BlockState blockStateOld = world.GetBlockState(blockPos);
                    bool result = world.SetBlockState(blockPos, blockState, 15);
                    if (result)
                    {
                    //InstallAdditionalBlocks(worldIn, blockPos);
                    //if (!playerIn.IsCreativeMode)
                    //{
                    //    blockStateOld.GetBlock().DropBlockAsItem(world, blockPos, blockStateOld);
                    //}
                    //if (!playerIn.IsCreativeMode)
                   // {
                      //  pla
//                        playerIn.Inventory.DecrStackSize(playerIn.Inventory.GetCurrentItem(), 1);
                  //  }
                   // Block.DecrStackSize(stack, playerIn);
                      //  world.PlaySound(playerIn, Block.SamplePut(worldIn), blockPos.ToVec3(), 1f, 1f);
                    }
                    return result;
                }
            }
            return false;
        }

        #endregion
    }
}
