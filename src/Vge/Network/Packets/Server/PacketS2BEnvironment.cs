namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Изменения погоды
    /// </summary>
    public struct PacketS2BEnvironment : IPacket
    {
        public byte Id => 0x2B;

        public byte Index { get; private set; }
        public float Parameter { get; private set; }

        public PacketS2BEnvironment(byte index, float parameter)
        {
            Index = index;
            Parameter = parameter;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Index = stream.Byte();
            Parameter = stream.Float();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte(Index);
            stream.Float(Parameter);
        }
    }
}
