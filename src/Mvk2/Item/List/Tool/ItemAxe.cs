using Mvk2.World.Block;
using System;
using Vge.Entity.Player;
using Vge.Item;
using Vge.World.Block;

namespace Mvk2.Item.List.Tool
{
    /// <summary>
    /// Предмет топор
    /// </summary>
    public class ItemAxe : ItemAbTool
    {
        /// <summary>
        /// Действие предмета ЛКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        public override ResultHandAction OnAction(bool begin, ItemStack stack, PlayerClientOwner player)
        {
            if (player.MovingObject.IsBlock())
            {
                // Опции для возможности копать, можно проверить типа блока player.MovingObject.Block
                BlockBase block = player.MovingObject.Block.GetBlock();
                if (player.CanDestroyedBlock(block) 
                    || block.Material.IndexMaterial == (int)EnumMaterial.Wood
                    || block.Material.IndexMaterial == (int)EnumMaterial.Door
                    || block.Material.IndexMaterial == (int)EnumMaterial.Branch)
                {
                    ItemStack itemStackLeft = player.Inventory.GetCurrentLeftItem();
                    if (itemStackLeft != null
                        && itemStackLeft.Item is ItemAxe itemAxe)
                    {
                        // Если второй топор есть, то усиление
                        player.DamageItemTool(1, true);
                        int pause = (itemAxe.Pause + Pause) / 2;
                        float destroy = Acceleration * itemAxe.Acceleration 
                            / (Acceleration + itemAxe.Acceleration);
                        
                        return new ResultHandAction(HitDelay, pause, destroy);
                    }

                    return new ResultHandAction(HitDelay, Pause, Acceleration);
                }
            }

            return base.OnAction(begin, stack, player);
        }
    }
}
