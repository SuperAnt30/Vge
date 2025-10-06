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
        /// <summary>
        /// Количество предметов добавляется к стаку, если оно равно как в стаке, значит целиком
        /// </summary>
        public readonly int Amount;

        public SlotEventArgs(int slot) => SlotId = slot;

        public SlotEventArgs(int slot, ItemStack stack)
        {
            SlotId = slot;
            Stack = stack;
        }

        public SlotEventArgs(int slot, int amount)
        {
            SlotId = slot;
            Amount = amount;
        }

        public SlotEventArgs(int slot, ItemStack stack, int amount)
        {
            SlotId = slot;
            Stack = stack;
            Amount = amount;
        }

        public override string ToString() => SlotId.ToString() + " " 
            + (Stack == null ? "Null" : Stack.ToString()) + " " + Amount.ToString();
    }
}
