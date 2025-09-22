using Vge.Item;

namespace Vge.Entity.Inventory
{
    /// <summary>
    /// Структура слота инвентаря и тайлентити, для NBT
    /// </summary>
    public struct Slot
    {
        /// <summary>
        /// Индекс слота
        /// </summary>
        public byte SlotId;
        /// <summary>
        /// Объект одной ячейки
        /// </summary>
        public ItemStack Stack;

        public Slot(byte slotId, ItemStack stack = null)
        {
            SlotId = slotId;
            Stack = stack;
        }

        public override string ToString() 
            => SlotId.ToString() + ": " + Stack.ToString();
    }
}
