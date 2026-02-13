using Vge.Entity.Player;
using Vge.Item;

namespace Vge.World.BlockEntity
{
    /// <summary>
    /// Интерфейс блок хранилища, если у блока имеется аналог инвентаря
    /// </summary>
    public interface IBlockStorage
    {
        /// <summary>
        /// Открыли окно, вызывается объектом PlayerServer
        /// </summary>
        void OpenWindow(PlayerServer entityPlayer);

        /// <summary>
        /// Получить стак в конкретном слоте
        /// </summary>
        ItemStack GetStackInSlot(int slotIn);

        /// <summary>
        /// Задать стак в конкретном слоте
        /// </summary>
        void SetStackInSlot(int slotIn, ItemStack stack);

        /// <summary>
        /// Добавляет стак предметов в инвентарь через Shift, возвращает false, если это невозможно.
        /// </summary>
        bool AddItemStackToInventory(ItemStack stack);

        /// <summary>
        /// Проверяем можно ли установить данный стак в определённой ячейке склада
        /// </summary>
        bool CanPutItemStack(int slotIn, ItemStack stack);

        /// <summary>
        /// Проверить равенства
        /// </summary>
        bool CheckEquals(IBlockStorage blockStorage);
    }
}
