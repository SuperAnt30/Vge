namespace Vge.Network.Packets.Server
{
    public struct PacketS04TimeUpdate : IPacket
    {
        public byte Id => 0x04;

        /// <summary>
        /// Время сервера
        /// </summary>
        public uint Time { get; private set; }

        public PacketS04TimeUpdate(uint time) => Time = time;


        public void ReadPacket(ReadPacket stream) => Time = stream.UInt();
        public void WritePacket(WritePacket stream) => stream.UInt(Time);
    }
}
