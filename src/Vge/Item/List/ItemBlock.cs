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
        /// Вспомогательное действие предмета ПКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        /// <param name="counter">Счётчик тактов от нажатия вспомогательное действия ПКМ</param>
        public override ResultHandSecond OnSecond(bool begin, ItemStack stack, 
            PlayerClientOwner player, int counter)
        {
            ResultHandSecond result = base.OnSecond(begin, stack, player, counter);
            if (result.Action != ResultHandSecond.ActionType.None)
            {
                return result;
            }

            MovingObjectPosition moving = player.MovingObject;
            if (moving.IsBlock())
            {
                if (OnItemOnBlockPlacement(stack, player, moving.BlockPosition, moving.Side, 
                    moving.Facing, true))
                {
                    return new ResultHandSecond(begin ? 10 : 5, true);
                }
                return new ResultHandSecond(ResultHandSecond.ActionType.None, begin ? 10 : 5);
            }
            return new ResultHandSecond(ResultHandSecond.ActionType.None);
        }

        /// <summary>
        /// Вызывается, когда текущий предмет пробуюет установить на блок,
        /// возвращает true если действие состоялось
        /// </summary>
        /// <param name="pos">Позиция блока, по которому щелкают ПКМ</param>
        /// <param name="side">Сторона, по которой щелкнули ПКМ</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        /// <param name="flagReplaceable">Надо ли проверять смещение на установку блока параметра IsReplaceable</param>
        public override bool OnItemOnBlockPlacement(ItemStack stack, PlayerBase player,
            BlockPos blockPos, Pole side, Vector3 facing, bool flagReplaceable)
        {
            WorldBase world = player.GetWorld();

            //if (!world.IsRemote) return false;

            if (flagReplaceable && !world.GetBlockState(blockPos).GetBlock().IsReplaceable)
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
                        if (!player.CreativeMode)
                        {
                        //    blockStateOld.GetBlock().DropBlockAsItem(world, blockPos, blockStateOld);
                            player.Inventory.DecrCurrentItem(1);
                        }
                        world.PlaySound(Block.Material.SamplePut(world.Rnd),
                            blockPos.ToVector3Center(), 1, 1, player.Id);
                    }
                    return result;
                }
            }
            return false;
        }

        #endregion
    }
}
