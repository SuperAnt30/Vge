using Mvk2.Entity.List;
using Vge.Entity.Inventory;

namespace Mvk2.Entity.Inventory
{
    /// <summary>
    /// Инвентарь игрока
    /// </summary>
    public class InventoryPlayerMvk : InventoryPlayer
    {
        /// <summary>
        /// Количество ячеек карманов, не меньше 2 (_requiredPocket)
        /// </summary>
        public const byte PocketCount = 12;// 6; // 8
        /// <summary>
        /// Количество ячеек одежды, первый слот это предмет левой руки 
        /// </summary>
        public const byte ClothCount = 10;//9; // 11
        /// <summary>
        /// Количество ячеек рюкзака
        /// </summary>
        public const byte BackpackCount = 35;//15; // 25

        public InventoryPlayerMvk(PlayerServerMvk playerServer)
            // Первый слот одеждый это ячейка левой руки
            : base(playerServer, PocketCount, ClothCount, BackpackCount) { }
    }
}
