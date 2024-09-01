namespace Vge.Network.Packets
{
    /// <summary>
    /// Пакет для проверки ping-а, запускает клиент
    /// </summary>
    public struct Packet00PingPong : IPacket
    {
        public byte GetId() => 0x00;

        private long clientTime;
        public long GetClientTime() => clientTime;

        public Packet00PingPong(long time) => clientTime = time;

        public void ReadPacket(ReadPacket stream) => clientTime = stream.Long();
        public void WritePacket(WritePacket stream) => stream.Long(clientTime);
    }
}
