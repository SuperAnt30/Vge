using Vge.Entity.Inventory;
using Vge.Item;

namespace Mvk2.Entity.Inventory
{
    /// <summary>
    /// Инвентарь сетевого игрока на клиентской стороне
    /// </summary>
    public class InventoryPlayerClientMvk : InventoryList
    {
        public InventoryPlayerClientMvk() : base(1, 
            InventoryPlayerMvk.ClothCount, 1 + InventoryPlayerMvk.ClothCount)
        {
            // Первый слот одеждый это ячейка левой руки
        }

        /// <summary>
        /// Получить текущий стак левой руки
        /// </summary>
        public override ItemStack GetCurrentLeftItem() => GetStackInSlot(_pocketCount);
    }
}
