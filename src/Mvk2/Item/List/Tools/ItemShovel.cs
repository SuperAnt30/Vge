using Mvk2.World.Block;
using Vge.Entity.Player;
using Vge.Item;
using Vge.World.Block;

namespace Mvk2.Item.List.Tools
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
        /// <param name="preview">Предпросмотр установки, true - без установки блока(ов)</param>
        /// <param name="begin">Первый клик, без паузы</param>
        public override ResultHandAction OnAction(bool preview, bool begin, ItemStack stack, PlayerClientOwner player)
        {
            if (player.MovingObject.IsBlock())
            {
                // Опции для возможности копать, можно проверить типа блока player.MovingObject.Block
                BlockBase block = player.MovingObject.Block.GetBlock();
                if (player.CanDestroyedBlock(block) 
                    || block.Material.IndexMaterial == (int)EnumMaterial.Loose)
                {
                    return new ResultHandAction(HitDelay, Pause, Acceleration);
                }
            }

            return base.OnAction(preview, begin, stack, player);
        }
    }
}
