//namespace Vge.Network.Packets.Server
//{
//    public struct PacketS01KeepAlive : IPacket
//    {
//        public byte GetId() => 0x01;

//        private uint time;
//        public uint GetTime() => time;

//        public PacketS01KeepAlive(uint time) => this.time = time;

//        public void ReadPacket(ReadPacket stream) => time = stream.UInt();
//        public void WritePacket(WritePacket stream) => stream.UInt(time);
//    }
//}
