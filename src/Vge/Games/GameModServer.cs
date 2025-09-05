using Vge.Entity.Player;
using Vge.Network;

namespace Vge.Games
{
    /// <summary>
    /// Объект игрового мода для серверной части, этот объект наследуется другими проектами
    /// </summary>
    public class GameModServer
    {
        /// <summary>
        /// Игровой сервер
        /// </summary>
        public GameServer Server { get; private set; }

        public GameModServer() { }

        public void Init(GameServer server) => Server = server;

        /// <summary>
        /// Инициализация прямо перед циклом сервера
        /// </summary>
        public virtual void InitBeforeLoop() { }

        /// <summary>
        /// Создать объект сетевого игрока
        /// </summary>
        public virtual PlayerServer CreatePlayerServer(string login, string token, SocketSide socketSide)
            => new PlayerServer(login, token, socketSide, Server);
    }
}
