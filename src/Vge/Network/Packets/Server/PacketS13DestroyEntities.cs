namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Уничтожение сущностей
    /// </summary>
    public struct PacketS13DestroyEntities : IPacket
    {
        public byte Id => 0x13;

        public int[] Ids { get; private set; }

        public PacketS13DestroyEntities(int[] ids) => Ids = ids;

        public void ReadPacket(ReadPacket stream)
        {
            int count = stream.Int();
            Ids = new int[count];
            for (int i = 0; i < count; i++)
            {
                Ids[i] = stream.Int();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(Ids.Length);
            for (int i = 0; i < Ids.Length; i++)
            {
                stream.Int(Ids[i]);
            }
        }
    }
}
