using Vge.Item;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет смены видимых предметов у сущности
    /// </summary>
    public struct PacketS10EntityEquipment : IPacket
    {
        public byte Id => 0x10;

        public int EntityId { get; private set; }
        public byte SlotId { get; private set; }
        public ItemStack Stack { get; private set; }

        public PacketS10EntityEquipment(int entityId, byte slotId, ItemStack itemStack)
        {
            EntityId = entityId;
            SlotId = slotId;
            Stack = itemStack;
        }

        public void ReadPacket(ReadPacket stream)
        {
            EntityId = stream.Int();
            SlotId = stream.Byte();
            Stack = ItemStack.ReadStream(stream);
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(EntityId);
            stream.Byte(SlotId);
            ItemStack.WriteStream(Stack, stream);
        }
    }
}
