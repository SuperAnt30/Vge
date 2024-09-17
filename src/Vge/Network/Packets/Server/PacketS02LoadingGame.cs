namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Загрузка мира, только для локальной
    /// </summary>
    public struct PacketS02LoadingGame : IPacket
    {
        public byte GetId() => 0x02;

        private bool begin;
        private ushort value;

        public bool IsBegin() => begin;
        public ushort GetValue() => value;

        public PacketS02LoadingGame(bool begin)
        {
            this.begin = begin;
            value = 0;
        }

        public PacketS02LoadingGame(ushort value)
        {
            begin = true;
            this.value = value;
        }

        public void ReadPacket(ReadPacket stream)
        {
            begin = stream.Bool();
            if (begin)
            {
                value = stream.UShort();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Bool(begin);
            if (begin)
            {
                stream.UShort(value);
            }
        }
    }
}
