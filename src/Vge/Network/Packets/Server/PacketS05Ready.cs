namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Передать готовность игры
    /// </summary>
    public struct PacketS05Ready : IPacket
    {
        public byte Id => 0x05;
        public void ReadPacket(ReadPacket stream) { }
        public void WritePacket(WritePacket stream) { }
    }
}
