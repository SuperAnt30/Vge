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

        /// <summary>
        /// Инициализация при создании сервера, в начале.
        /// </summary>
        public virtual void Init(GameServer server) => Server = server;

        /// <summary>
        /// Вторая инициализация когда готовы предметы и блоки и сущности
        /// </summary>
        public virtual void InitTwo() { }

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
