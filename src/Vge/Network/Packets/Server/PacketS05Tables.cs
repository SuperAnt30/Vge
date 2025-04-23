namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Передать таблицы
    /// </summary>
    public struct PacketS05Tables : IPacket
    {
        public byte Id => 0x05;

        /// <summary>
        /// Массив блоков
        /// </summary>
        public string[] Blocks { get; private set; }
        /// <summary>
        /// Массив сущностей
        /// </summary>
        public string[] Entities { get; private set; }

        /// <summary>
        /// Передать таблицу блоков
        /// </summary>
        public PacketS05Tables(string[] blocks, string[] entities)
        {
            Blocks = blocks;
            Entities = entities;
        }

        public void ReadPacket(ReadPacket stream)
        {
            ushort count = stream.UShort();
            Blocks = new string[count];
            for (int i = 0; i < count; i++)
            {
                Blocks[i] = stream.String();
            }
            count = stream.UShort();
            Entities = new string[count];
            for (int i = 0; i < count; i++)
            {
                Entities[i] = stream.String();
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
            count = (ushort)Entities.Length;
            stream.UShort(count);
            for (int i = 0; i < count; i++)
            {
                stream.String(Entities[i]);
            }
        }
    }
}
