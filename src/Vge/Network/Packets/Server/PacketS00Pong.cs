//namespace Vge.Network.Packets.Server
//{
//    public struct PacketS00Pong : IPacket
//    {
//        public byte GetId() => 0x00;

//        private long clientTime;

//        public long GetClientTime() => clientTime;

//        public PacketS00Pong(long time) => clientTime = time;

//        public void ReadPacket(ReadPacket stream) => clientTime = stream.Long();
//        public void WritePacket(WritePacket stream) => stream.Long(clientTime);
//    }
//}
