namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Изменения погоды
    /// </summary>
    public struct PacketS2BEnvironment : IPacket
    {
        public byte Id => 0x2B;

        /// <summary>
        /// 0-5 EnumClouds
        /// 6 - Гром
        /// 7-9 EnumPrecipitation + 7
        /// </summary>
        public byte EnvironmentId { get; private set; }

        public PacketS2BEnvironment(byte environmentId)
        {
            EnvironmentId = environmentId;
        }

        public void ReadPacket(ReadPacket stream)
        {
            EnvironmentId = stream.Byte();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte(EnvironmentId);
        }
    }
}
