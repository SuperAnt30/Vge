namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Игрок соединился к серверу отправляем его index и uuid
    /// </summary>
    public struct PacketS03JoinGame : IPacket
    {
        public byte GetId() => 0x03;

        private int index;
        private string uuid;

        public int GetIndex() => index;
        public string GetUuid() => uuid;

        public PacketS03JoinGame(int index, string uuid)
        {
            this.index = index;
            this.uuid = uuid;
        }

        public void ReadPacket(ReadPacket stream)
        {
            index = stream.Int();
            uuid = stream.String();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(index);
            stream.String(uuid);
        }
    }
}
