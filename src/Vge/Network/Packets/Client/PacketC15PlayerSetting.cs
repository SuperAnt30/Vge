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
        private byte _overviewChunk;

        public byte GetOverviewChunk() => _overviewChunk;

        public PacketC15PlayerSetting(byte overviewChunk) => _overviewChunk = overviewChunk;

        public void ReadPacket(ReadPacket stream) => _overviewChunk = stream.Byte();
        public void WritePacket(WritePacket stream) => stream.Byte(_overviewChunk);
    }
}
