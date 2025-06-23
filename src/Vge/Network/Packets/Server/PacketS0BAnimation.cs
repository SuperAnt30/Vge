namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет анимации
    /// </summary>
    public struct PacketS0BAnimation : IPacket
    {
        public byte Id => 0x0B;

        public int EntityId { get; private set; }
        public byte Animation { get; private set; }

        public PacketS0BAnimation(int entityId, byte animation)
        {
            EntityId = entityId;
            Animation = animation;
        }

        public void ReadPacket(ReadPacket stream)
        {
            EntityId = stream.Int();
            Animation = stream.Byte();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(EntityId);
            stream.Byte(Animation);
        }
    }
}
