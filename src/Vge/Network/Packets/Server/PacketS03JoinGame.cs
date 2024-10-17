namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Игрок соединился к серверу отправляем его index и uuid
    /// </summary>
    public struct PacketS03JoinGame : IPacket
    {
        public byte Id => 0x03;

        public int Index { get; private set; }
        public string Uuid { get; private set; }

        public PacketS03JoinGame(int index, string uuid)
        {
            Index = index;
            Uuid = uuid;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Index = stream.Int();
            Uuid = stream.String();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(Index);
            stream.String(Uuid);
        }
    }
}
