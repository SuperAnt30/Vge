using Vge.Entity.Inventory;
using Vge.Item;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Управление передвежением и изменением слота
    /// </summary>
    public struct PacketS2FSetSlot : IPacket
    {
        public byte Id => 0x2F;

        public short SlotId { get; private set; }
        public ItemStack Stack { get; private set; }

        public PacketS2FSetSlot(Slot slot)
        {
            SlotId = slot.Index;
            Stack = slot.Stack;
        }

        public Slot GetSlot() => new Slot(SlotId, Stack);

        public void ReadPacket(ReadPacket stream)
        {
            SlotId = stream.Short();
            bool body = stream.Bool();
            Stack = body ? ItemStack.ReadStream(stream) : null;
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Short(SlotId);
            bool body = Stack != null;
            stream.Bool(body);
            if (body)
            {
                ItemStack.WriteStream(Stack, stream);
            }
        }
    }
}
