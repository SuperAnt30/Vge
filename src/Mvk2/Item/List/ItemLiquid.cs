using System;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Item.List;
using Vge.Json;
using Vge.Realms;
using Vge.Renderer;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Block.List;
using WinGL.Util;

namespace Mvk2.Item.List
{
    /// <summary>
    /// Предмет установить блок жидкости
    /// </summary>
    public class ItemLiquid : ItemBlock
    {
        /// <summary>
        /// Целый ли блок, и часть воды
        /// Целый 0. Часть 1 - 7
        /// </summary>
        private bool _flagFull;
        /// <summary>
        /// Смещение текстуры 
        /// </summary>
        private readonly float _biasTexture;

        /// <param name="full">Установка полного блока</param>
        public ItemLiquid(BlockLiquid blockLiquid, bool full, float biasTexture = 0) : base(blockLiquid)
        {
            _flagFull = full;
            _biasTexture = biasTexture;
        }

        /// <summary>
        /// Задать подсказку
        /// </summary>
        public override void SetToolTipLang(string toolTip)
        {
            base.SetToolTipLang(toolTip);
            _toolTip = _toolTip + ChatStyle.Br + "-" + ChatStyle.Br
                + L.T("Liter") + " {0}" + ChatStyle.Br
                + "Damage {1}";
        }

        /// <summary>
        /// Текст в подсказке для GUI
        /// </summary>
        public override string GetToolTip(ItemStack stack)
            => string.Format(_toolTip, stack.Met * 2, stack.ToStringDamaged());

        /// <summary>
        /// Создать структура блок да установки
        /// </summary>
        protected override BlockState CreateBlockState(ItemStack stack)
        {
            if (_flagFull) 
            {
                return new BlockState(Block.IndexBlock);
            }
            // 0 - Значит максимум
            // 7 - Минимум
            int water = stack.Met;
            return new BlockState(Block.IndexBlock | water << 12);
        }

        /// <summary>
        /// Проверка равенства старого и нового блока, при установке
        /// </summary>
        protected override bool _CheckEqualsBlockPlace(BlockBase block, BlockBase blockOld) 
            => false; // Из-за доп житкости в блоке, блоки могут не меняться


        /// <summary>
        /// Вспомогательное действие предмета ПКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        /// <param name="counter">Счётчик тактов от нажатия вспомогательное действия ПКМ</param>
        public override ResultHandSecond OnSecond(bool begin, ItemStack stack, PlayerClientOwner player, int counter)
        {
            ResultHandSecond result = _CheckEntitySecond(player);
            if (result.Action != ResultHandSecond.ActionType.None)
            {
                return result;
            }

            MovingObjectPosition moving = player.MovingObject;
            if (moving.IsLiquid)
            {
                if (moving.IdBlockLiquid == Block.IndexBlock
                    && OnItemOnBlockPlacement(stack, player, moving.BlockLiquidPosition, moving.Side,
                    moving.Facing, true, player.RotationYaw))
                {
                    return new ResultHandSecond(ResultHandSecond.ActionType.BlockPlacement, true);
                }
            }
            else if (moving.IsBlock())
            {
                // Надо проверить наличие воды
                if (OnItemOnBlockPlacement(stack, player, moving.BlockPosition, moving.Side,
                    moving.Facing, true, player.RotationYaw))
                {
                    return new ResultHandSecond(ResultHandSecond.ActionType.BlockPlacement, false);
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
        public override bool OnItemOnBlockPlacement(ItemStack stack, PlayerBase player,
            BlockPos blockPos, Pole side, Vector3 facing, bool flagReplaceable, float playerYaw)
        {
            if (_flagFull)
            {
                // Установка блока воды
                return base.OnItemOnBlockPlacement(stack, player, blockPos, side, facing, flagReplaceable, playerYaw);
            }

            WorldBase world = player.GetWorld();
            BlockState blockState = player.GetWorld().GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            int water = 0;
            int idBlockLiquid = -1;

            if (block.Material.Liquid)
            {
                water = blockState.Met & 0xF;
                if (water == 0 || water > 7) water = 7;
                idBlockLiquid = block.IndexBlock;
            }
            else if (block.IsAddLiquid(blockState.Met))
            {
                water = block.GetAddLiquidMet(blockState.Met);
                if (water == 0 || water > 7) water = 7;
                idBlockLiquid = block.GetAddLiquid(blockState.Met).IndexBlock;
            }

            // Доп проверка, если в бок, нужен соседний блок
            if (idBlockLiquid == -1 && flagReplaceable && !block.IsReplaceable)
            {
                // Делаем смещение и повторяем всю процедуру с флагом о запрете смещения, защита от рекурсии
                blockPos = blockPos.Offset(side);
                return OnItemOnBlockPlacement(stack, player, blockPos, side, facing, false, playerYaw);
            }

            if (idBlockLiquid == Block.IndexBlock || idBlockLiquid == -1)
            {
                if (water != 0)
                {
                    // Имеетса вода пробуем забрать
                    // Console.WriteLine((world.IsRemote ? "C" : "S") + " !Water " + water);

                    if (stack != null) // по сути пустоты не должно быть!
                    {
                        if (stack.Met < 7)
                        {
                            water += stack.Met;
                            if (water > 7) water = 7;
                            stack.SetMet((byte)water);
                            player.Inventory.SetCurrentItem(stack);
                            world.SetLiquidBlockToAir(blockPos);
                            return true;
                        }
                    }
                }
                //else
                //{
                //    Console.Write((world.IsRemote ? "C" : "S") + " !Water No");
                //}

                if (base.OnItemOnBlockPlacement(stack, player, blockPos, side, facing, flagReplaceable, playerYaw))
                {
                    if (!player.CreativeMode)
                    {
                        // Вылили ведро
                        ItemStack stackLiquit = new ItemStack(ItemsRegMvk.Bucket, 1, stack.ItemDamaged);
                        player.Inventory.SetCurrentItem(stackLiquit);
                        // Инструмент ламаем
                        stackLiquit.DamageItemTool(player, 1, false);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Нарисовать сетку буффера дополнительный дааных для GUI
        /// </summary>
        public override float[] RenderBufferMetGui(byte met, int x, int y, int si)
        {
            float u = (met - 1) * .03125f + _biasTexture;
            return RenderFigure.Rectangle(
                    (x + 16) * si, (y + 0) * si, (x + 32) * si, (y + 32) * si,
                    u, 0, u + .03125f, 0.0625f,
                    1f, 1f, 1f);
        }

        /// <summary>
        /// Имеется ли сетка контура для предмета
        /// </summary>
        public override bool CanOutlineBlock() => false;

        /// <summary>
        /// Получить сетку контура предмета
        /// </summary>
        public override YouCanPlaceIt GetOutlineBlock(ItemStack stack, PlayerBase player,
            BlockPos blockPos, Pole side, Vector3 facing) => new YouCanPlaceIt();
    }
}
