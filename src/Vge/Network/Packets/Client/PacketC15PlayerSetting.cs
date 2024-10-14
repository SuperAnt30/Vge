namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Параметры игрока
    /// </summary>
    public struct PacketC15PlayerSetting : IPacket
    {
        public byte GetId() => 0x15;

        /// <summary>
        /// Обзор чанков
        /// </summary>
        public byte OverviewChunk { get; private set; }

        public PacketC15PlayerSetting(byte overviewChunk) => OverviewChunk = overviewChunk;

        public void ReadPacket(ReadPacket stream) => OverviewChunk = stream.Byte();
        public void WritePacket(WritePacket stream) => stream.Byte(OverviewChunk);
    }
}
