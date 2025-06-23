namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет анимации
    /// </summary>
    public struct PacketC0APlayerAnimation : IPacket
    {
        public byte Id => 0x0A;

        public byte Animation { get; private set; }

        public PacketC0APlayerAnimation(byte animation)
        {
            Animation = animation;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Animation = stream.Byte();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte(Animation);
        }
    }
}
