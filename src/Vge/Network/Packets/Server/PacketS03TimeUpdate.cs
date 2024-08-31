namespace Vge.Network.Packets.Server
{
    public struct PacketS03TimeUpdate : IPacket
    {
        public byte GetId() => 0x03;

        private uint time;

        public PacketS03TimeUpdate(uint time) => this.time = time;

        /// <summary>
        /// Время сервера
        /// </summary>
        public uint GetTime() => time;

        public void ReadPacket(ReadPacket stream) => time = stream.UInt();
        public void WritePacket(WritePacket stream) => stream.UInt(time);
    }
}
