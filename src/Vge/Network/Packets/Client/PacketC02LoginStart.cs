namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет расположения игрока
    /// </summary>
    public struct PacketC02LoginStart : IPacket
    {
        public byte Id => 0x02;

        /// <summary>
        /// Имя игрока
        /// </summary>
        public string Login { get; private set; }
        /// <summary>
        /// Имя игрока
        /// </summary>
        public string Token { get; private set; }
        /// <summary>
        /// Версия проекта
        /// </summary>
        public ushort Version { get; private set; }

        public PacketC02LoginStart(string login, string token, ushort version)
        {
            Login = login;
            Token = token;
            Version = version;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Login = stream.String();
            Token = stream.String();
            Version = stream.UShort();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.String(Login);
            stream.String(Token);
            stream.UShort(Version);
        }
    }
}
