namespace Vge.Entity.Inventory
{
    public delegate void SlotEventHandler(object sender, SlotEventArgs e);
    public class SlotEventArgs
    {
        /// <summary>
        /// Индекс измёнённого слота
        /// </summary>
        public readonly int IndexSlot;

        public SlotEventArgs(int slot) => IndexSlot = slot;
    }
}
