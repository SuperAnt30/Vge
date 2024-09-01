namespace Vge.Network.Packets
{
    /// <summary>
    /// Пакет для сохранения жизни, запускает сервер
    /// </summary>
    public struct Packet01KeepAlive : IPacket
    {
        public byte GetId() => 0x01;

        private uint time;
        public uint GetTime() => time;

        public Packet01KeepAlive(uint time) => this.time = time;

        public void ReadPacket(ReadPacket stream) => time = stream.UInt();
        public void WritePacket(WritePacket stream) => stream.UInt(time);
    }
}
