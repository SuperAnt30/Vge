namespace Vge.Network.Packets.Client
{
    public struct PacketC01KeepAlive : IPacket
    {
        public byte GetId() => 0x01;

        private uint time;
        public uint GetTime() => time;

        public PacketC01KeepAlive(uint time) => this.time = time;

        public void ReadPacket(ReadPacket stream) => time = stream.UInt();
        public void WritePacket(WritePacket stream) => stream.UInt(time);
    }
}
