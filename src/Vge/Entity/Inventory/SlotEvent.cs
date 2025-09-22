using Vge.Item;

namespace Vge.Entity.Inventory
{
    public delegate void SlotEventHandler(object sender, SlotEventArgs e);
    public class SlotEventArgs
    {
        /// <summary>
        /// Индекс измёнённого слота
        /// </summary>
        public readonly int SlotId;
        /// <summary>
        /// Объект одной ячейки
        /// </summary>
        public readonly ItemStack Stack;

        public SlotEventArgs(int slot) => SlotId = slot;

        public SlotEventArgs(int slot, ItemStack stack)
        {
            SlotId = slot;
            Stack = stack;
        }
    }
}
