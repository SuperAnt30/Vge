using Vge.Command;
using Vge.Entity;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Network;
using Vge.World.Block;
using Vge.World.BlockEntity;

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
        /// Корректировка блоков, сущностей и прочего перед инициализации миров, 
        /// тут для сервера и/или сингла!
        /// Для инициализация ID сущностей и подобного.
        /// </summary>
        public virtual void CorrectObjects()
        {
            BlocksReg.Correct(Server.Settings.TableBlocks);
            ItemsReg.Correct(Server.Settings.TableItems);
            EntitiesReg.Correct(Server.Settings.TableEntities);
            BlocksEntityReg.Correct(Server.Settings.TableBlocksEntity);
        }

        /// <summary>
        /// Создать объект сетевого игрока
        /// </summary>
        public virtual PlayerServer CreatePlayerServer(string login, string token, SocketSide socketSide)
            => new PlayerServer(login, token, socketSide, Server);

        /// <summary>
        /// Инициализация, регистрация комманд
        /// </summary>
        public virtual void InitCommand(ManagerCommand managerCommand) { }
    }
}
