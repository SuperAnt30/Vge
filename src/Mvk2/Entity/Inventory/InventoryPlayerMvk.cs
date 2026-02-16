using Mvk2.Entity.List;
using Mvk2.Item;
using Vge.Entity.Inventory;
using Vge.Item;

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
        public const byte ClothCount = 8;
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

        /// <summary>
        /// Возвращает стак слота, из инвентаря или хранилища выбранного блока
        /// </summary>
        protected override ItemStack _GetStackInSlotAndStorage(int slotIn)
        {
            if (slotIn > 99)
            {
                // слот склада
                if (_blockStorage != null)
                {
                    if (_isCreative && _blockStorage is ItemsCreative creative)
                    {
                        return creative.GetStackInSlot(_creativeTab, _creativePage, slotIn - 100);
                    }
                    return _blockStorage.GetStackInSlot(slotIn - 100);
                }
                return null;
            }
            // слот у игрока
            return GetStackInSlot(slotIn);
        }
    }
}
