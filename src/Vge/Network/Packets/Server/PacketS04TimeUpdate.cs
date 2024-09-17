namespace Vge.Network.Packets.Server
{
    public struct PacketS04TimeUpdate : IPacket
    {
        public byte GetId() => 0x04;

        private uint time;

        public PacketS04TimeUpdate(uint time) => this.time = time;

        /// <summary>
        /// Время сервера
        /// </summary>
        public uint GetTime() => time;

        public void ReadPacket(ReadPacket stream) => time = stream.UInt();
        public void WritePacket(WritePacket stream) => stream.UInt(time);
    }
}
