namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Передать таблицу блоков
    /// </summary>
    public struct PacketS05TableBlocks : IPacket
    {
        public byte Id => 0x05;

        /// <summary>
        /// Массив блоков
        /// </summary>
        public string[] Blocks { get; private set; }

        /// <summary>
        /// Передать таблицу блоков
        /// </summary>
        public PacketS05TableBlocks(string[] blocks)
            => Blocks = blocks;

        public void ReadPacket(ReadPacket stream)
        {
            ushort count = stream.UShort();
            Blocks = new string[count];
            for (int i = 0; i < count; i++)
            {
                Blocks[i] = stream.String();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            ushort count = (ushort)Blocks.Length;
            stream.UShort(count);
            for (int i = 0; i < count; i++)
            {
                stream.String(Blocks[i]);
            }
        }
    }
}
