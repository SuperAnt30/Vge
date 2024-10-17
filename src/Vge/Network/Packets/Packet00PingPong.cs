namespace Vge.Network.Packets
{
    /// <summary>
    /// Пакет для проверки ping-а, запускает клиент
    /// </summary>
    public struct Packet00PingPong : IPacket
    {
        public byte Id => 0x00;

        public long ClientTime { get; private set; }

        public Packet00PingPong(long time) => ClientTime = time;

        public void ReadPacket(ReadPacket stream) => ClientTime = stream.Long();
        public void WritePacket(WritePacket stream) => stream.Long(ClientTime);
    }
}
