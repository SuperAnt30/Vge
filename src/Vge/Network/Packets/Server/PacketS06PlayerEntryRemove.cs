namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет игрок зашёл или вышел из сервера, для чата
    /// </summary>
    public struct PacketS06PlayerEntryRemove : IPacket
    {
        public byte Id => 0x06;

        public int Index { get; private set; }
        /// <summary>
        /// Псевдоним игрока, если игрок вышел Login = ""
        /// </summary>
        public string Login { get; private set; }

        /// <summary>
        /// Игрок зашёл
        /// </summary>
        public PacketS06PlayerEntryRemove(int index, string login)
        {
            Index = index;
            Login = login;
        }
        /// <summary>
        /// Игрок вышел
        /// </summary>
        public PacketS06PlayerEntryRemove(int index)
        {
            Index = index;
            Login = "";
        }

        public void ReadPacket(ReadPacket stream)
        {
            Index = stream.Int();
            Login = stream.String();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(Index);
            stream.String(Login);
        }
    }
}
