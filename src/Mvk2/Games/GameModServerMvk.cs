using Mvk2.Command;
using Mvk2.Entity;
using Mvk2.Entity.List;
using Mvk2.World.BlockEntity;
using Vge.Command;
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

        /// <summary>
        /// Корректировка блоков, сущностей и прочего перед инициализации миров, 
        /// тут для сервера и/или сингла!
        /// Для инициализация ID сущностей и подобного.
        /// </summary>
        public override void CorrectObjects()
        {
            base.CorrectObjects();
            // Присвоение корректных ID
            EntitiesRegMvk.InitId();
            BlocksEntityRegMvk.InitId();
        }

        /// <summary>
        /// Инициализация, регистрация комманд
        /// </summary>
        public override void InitCommand(ManagerCommand managerCommand)
            => ManagerCommandMvk.Init(Server, managerCommand);
    }
}
