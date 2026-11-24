using Mvk2.World.Block;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Realms;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Mvk2.Item.List.Blocks
{
    /// <summary>
    /// Предмет пустое ведро
    /// </summary>
    public class ItemBucket : ItemBase
    {
        public ItemBucket() { } 

        /// <summary>
        /// Задать подсказку
        /// </summary>
        public override void SetToolTipLang(string toolTip)
        {
            base.SetToolTipLang(toolTip);
            _toolTip = _toolTip + ChatStyle.Br + "-" + ChatStyle.Br
                + L.T("Liter") + " 0" + ChatStyle.Br
                + "Damage {0}";
        }

        /// <summary>
        /// Текст в подсказке для GUI
        /// </summary>
        public override string GetToolTip(ItemStack stack)
            => string.Format(_toolTip, stack.ToStringDamaged());

        /// <summary>
        /// Вспомогательное действие предмета ПКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="preview">Предпросмотр установки, true - без установки блока(ов)</param>
        /// <param name="begin">Первый клик, без паузы</param>
        /// <param name="counter">Счётчик тактов от нажатия вспомогательное действия ПКМ</param>
        public override ResultHandSecond OnSecond(bool preview, bool begin, ItemStack stack, PlayerClientOwner player, int counter)
        {
            ResultHandSecond result = _CheckEntitySecond(player);
            if (result.Action != ResultHandSecond.ActionType.None)
            {
                return result;
            }

            MovingObjectPosition moving = player.MovingObject;
            if (moving.IsLiquid)
            {
                if (OnItemOnBlockPlacement(preview, stack, player, moving.BlockLiquidPosition, moving.Side,
                    moving.Facing, true, player.RotationYaw))
                {
                    return new ResultHandSecond(HandManager.PauseUseItem, true)
                    {
                        IsLiquid = true,
                        UseDelay = HandManager.SecondDelay
                    };
                }
            }
            else if (moving.IsBlock())
            {
                // Надо проверить наличие воды
                if (moving.Block.GetBlock().IsAddLiquid(moving.Block.Met)
                    && OnItemOnBlockPlacement(preview, stack, player, moving.BlockPosition, moving.Side,
                    moving.Facing, true, player.RotationYaw))
                {
                    return new ResultHandSecond(HandManager.PauseUseItem, true)
                    {
                        UseDelay = HandManager.SecondDelay
                    };
                }
            }

            player.PlaySampleNotCanPlacement();
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
        /// <param name="playerYaw">Угол Yaw игрока в момент установки блока</param>
        public override bool OnItemOnBlockPlacement(bool preview, ItemStack stack, PlayerBase player,
            BlockPos blockPos, Pole side, Vector3 facing, bool flagReplaceable, float playerYaw)
        {
            if (!preview)
            {
                WorldBase world = player.GetWorld();


                BlockState blockState = player.GetWorld().GetBlockState(blockPos);
                BlockBase block = blockState.GetBlock();
                int water;
                int idBlockLiquid;

                if (block.Material.Liquid)
                {
                    water = blockState.Met & 0xF;
                    idBlockLiquid = block.IndexBlock;
                }
                else
                {
                    water = block.GetAddLiquidMet(blockState.Met);
                    idBlockLiquid = block.GetAddLiquid(blockState.Met).IndexBlock;
                }

                if (water == 0 || water > 7) water = 7;

                //  Console.WriteLine((world.IsRemote ? "C" : "S") + " Water " + water);

                if (!player.CreativeMode)
                {
                    if (stack != null) // по сути пустоты не должно быть!
                    {
                        ItemLiquid item = BlocksRegMvk.Lava.IndexBlock == idBlockLiquid
                            ? ItemsRegMvk.BucketLava : ItemsRegMvk.BucketWater;

                        ItemStack stackLiquit = new ItemStack(item, 1,
                            stack.ItemDamaged, (byte)water);
                        //stack.DamageItemTool(player, water, true);

                        if (stack.Amount > 1)
                        {
                            // Много ведёр, будет добавлять
                            player.Inventory.DecrCurrentItem(1);
                            player.AddItemStackToInventory(stackLiquit);
                        }
                        else
                        {
                            // Одно ведро меняем этот же стак
                            player.Inventory.SetCurrentItem(stackLiquit);
                        }
                    }
                }
                world.SetLiquidBlockToAir(blockPos);
            }
            return true;
        }
    }
}
