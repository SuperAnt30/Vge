using Mvk2.Entity.List;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Network;

namespace Mvk2.Games
{
    /// <summary>
    /// Объект игрового мода для серверной части, для Малювеки 2
    /// </summary>
    public class GameModServerMvk : GameModServer
    {
        public GameModServerMvk() : base() { }

        /// <summary>
        /// Создать объект сетевого игрока
        /// </summary>
        public override PlayerServer CreatePlayerServer(string login, string token, SocketSide socketSide)
            => new PlayerServerMvk(login, token, socketSide, Server);
    }
}
