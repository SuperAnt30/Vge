using Mvk2.World.Block;
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
                    || block.Material.IndexMaterial == (int)EnumMaterial.Branch)
                {
                    // TODO::2026-10-14 Если второй топор есть, то усиление

                    ItemStack itemStackLeft = player.Inventory.GetCurrentLeftItem();
                    if (itemStackLeft != null
                        && itemStackLeft.Item is ItemAxe itemAxe)
                    {
                        //itemAxe.Pause; // Надо уменьшить
                        //itemAxe.Acceleration; // Надо уменьшить

                        return new ResultHandAction(Pause, Acceleration / 2f);
                    }

                    return new ResultHandAction(Pause, Acceleration);
                }
            }

            return base.OnAction(begin, stack, player);
        }
    }
}
