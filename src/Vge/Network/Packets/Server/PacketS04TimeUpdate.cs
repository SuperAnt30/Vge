namespace Vge.Network.Packets.Server
{
    public struct PacketS04TickUpdate : IPacket
    {
        public byte Id => 0x04;

        /// <summary>
        /// Время сервера
        /// </summary>
        public uint Tick { get; private set; }

        public PacketS04TickUpdate(uint tick) => Tick = tick;

        public void ReadPacket(ReadPacket stream) => Tick = stream.UInt();
        public void WritePacket(WritePacket stream) => stream.UInt(Tick);
    }
}
