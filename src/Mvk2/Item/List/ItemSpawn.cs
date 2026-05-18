using Mvk2.Entity;
using System;
using Vge.Entity;
using Vge.Entity.Player;
using Vge.Item;
using Vge.World;

namespace Mvk2.Item.List
{
    /// <summary>
    /// Предмет для спавна сущности
    /// </summary>
    public class ItemSpawn : ItemBase
    {
        /// <summary>
        /// Действие предмета ЛКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        public override ResultHandAction OnAction(bool begin, ItemStack stack, PlayerClientOwner player)
        {
            if (!player.CreativeMode)
            {
                player.Inventory.DecrCurrentItem(1);
            }
            return new ResultHandAction(ResultHandAction.ActionType.UseItem, 12);
        }

        /// <summary>
        /// Действие текущего предмета.
        /// Для сервера.
        /// </summary>
        public override void OnUseAction(ItemStack stack, PlayerServer player)
        {
            player.DropItem(stack.Copy(1), true, false);
            if (!player.CreativeMode)
            {
                player.Inventory.DecrCurrentItem(1);
            }
        }

        /// <summary>
        /// Вспомогательное действие предмета ПКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        /// /// <param name="counter">Счётчик тактов от нажатия вспомогательное действия ПКМ</param>
        public override ResultHandSecond OnSecond(bool begin, ItemStack stack,
            PlayerClientOwner player, int counter)
        {
            ResultHandSecond result = base.OnSecond(begin, stack, player, counter);
            if (result.Action == ResultHandSecond.ActionType.None || player.IsKeyShift())
            {
                if (!player.CreativeMode)
                {
                    player.Inventory.DecrCurrentItem(1);
                }
                return new ResultHandSecond(ResultHandSecond.ActionType.UseItem, 12);
            }
            return result;
        }

        /// <summary>
        /// Вспомогательное действие текущего предмета.
        /// Для сервера
        /// </summary>
        /// <param name="number">Дополнительный цифровой параметр</param>
        public override void OnUseSecond(ItemStack stack, PlayerServer player, int number)
        {
            WorldServer worldServer = player.GetWorldServer();
            if (worldServer != null)
            {
                EntityBase entity = Ce.Entities.CreateEntityServer(EntitiesRegMvk.ChickenId, worldServer);
                //if (entity is EntityItem entityItem)
                //{
                //    entityItem.SetEntityItemStack(itemStack);
                //}
                worldServer.EntityDropsEntityInWorld(player, entity, true, false);
            }

            //player.DropItem(stack.Copy(1), true, false);
            //if (!player.CreativeMode)
            //{
            //    player.Inventory.DecrCurrentItem(1);
            //}
        }
    }
}
