using Mvk2.Entity.List;
using Mvk2.Item;
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
        public const byte PocketCount = 12;
        /// <summary>
        /// Количество ячеек одежды, первый слот это предмет левой руки 
        /// </summary>
        public const byte ClothCount = 10;
        /// <summary>
        /// Количество ячеек рюкзака
        /// </summary>
        public const byte BackpackCount = 35;

        public InventoryPlayerMvk(PlayerServerMvk playerServer)
            // Первый слот одеждый это ячейка левой руки
            : base(playerServer, PocketCount, ClothCount, BackpackCount)
        {
            // Левая рука находится всегда в первом слоте массива одежды
            // Это присвоение 0 надо для того чтоб любой предмет можно было брать в эту руку
            //_slotClothKeys[0] = 0; // (int)EnumCloth.HandLeft - 1
        }
    }
}
