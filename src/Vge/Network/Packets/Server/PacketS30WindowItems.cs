using Vge.Item;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет окна списка предметов, стартово все придметы игрока
    /// </summary>
    public struct PacketS30WindowItems : IPacket
    {
        public byte Id => 0x30;

        public bool IsInventory { get; private set; }
        public ItemStack[] Stacks { get; private set; }

        public PacketS30WindowItems(bool isInventory, ItemStack[] stacks)
        {
            IsInventory = isInventory;
            Stacks = stacks;
        }

        public void ReadPacket(ReadPacket stream)
        {
            IsInventory = stream.Bool();
            int count = stream.UShort();
            Stacks = new ItemStack[count];
            for (int i = 0; i < count; i++)
            {
                Stacks[i] = ItemStack.ReadStream(stream);
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Bool(IsInventory);
            int count = Stacks.Length;
            stream.UShort((ushort)count);
            for (int i = 0; i < count; i++)
            {
                ItemStack.WriteStream(Stacks[i], stream);
            }
        }
    }
}
