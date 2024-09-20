namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет расположения игрока
    /// </summary>
    public struct PacketC02LoginStart : IPacket
    {
        public byte GetId() => 0x02;

        private string login;
        private string token;
        private ushort version;

        /// <summary>
        /// Имя игрока
        /// </summary>
        public string GetLogin() => login;
        /// <summary>
        /// Имя игрока
        /// </summary>
        public string GetToken() => token;
        /// <summary>
        /// Версия проекта
        /// </summary>
        public ushort GetVersion() => version;

        public PacketC02LoginStart(string login, string token, ushort version)
        {
            this.login = login;
            this.token = token;
            this.version = version;
        }

        public void ReadPacket(ReadPacket stream)
        {
            login = stream.String();
            token = stream.String();
            version = stream.UShort();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.String(login);
            stream.String(token);
            stream.UShort(version);
        }
    }
}
