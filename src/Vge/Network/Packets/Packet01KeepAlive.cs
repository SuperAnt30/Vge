namespace Vge.Network.Packets
{
    /// <summary>
    /// Пакет для сохранения жизни, запускает сервер
    /// </summary>
    public struct Packet01KeepAlive : IPacket
    {
        public byte Id => 0x01;

        public uint Time { get; private set; }

        public Packet01KeepAlive(uint time) => Time = time;

        public void ReadPacket(ReadPacket stream) => Time = stream.UInt();
        public void WritePacket(WritePacket stream) => stream.UInt(Time);
    }
}
