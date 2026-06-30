using Mvk2.World.Block;
using Vge.Entity.Player;
using Vge.Item;
using Vge.World.Block;

namespace Mvk2.Item.List.Tool
{
    /// <summary>
    /// Предмет лопата
    /// </summary>
    public class ItemShovel : ItemAbTool
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
                    || block.Material.IndexMaterial == (int)EnumMaterial.Loose)
                {
                    return new ResultHandAction(_pause, _acceleration);
                }
            }

            return base.OnAction(begin, stack, player);
        }
    }
}
