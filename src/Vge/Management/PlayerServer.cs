using Vge.Games;
using Vge.Network;

namespace Vge.Management
{
    /// <summary>
    /// Объект игрока, как локального так и сетевого, не сущность
    /// </summary>
    public class PlayerServer
    {
        public readonly SocketSide Socket;
        public readonly bool owner;
        /// <summary>
        /// Основной сервер
        /// </summary>
        private readonly Server server;

        public string Login;

        public PlayerServer(SocketSide socket, Server server)
        {
            Socket = socket;
            owner = socket == null;
            this.server = server;
        }

        /// <summary>
        /// Отправить сетевой пакет этому игроку
        /// </summary>
        public void SendPacket(IPacket packet)
            => server.ResponsePacket(Socket, packet);
    }
}
