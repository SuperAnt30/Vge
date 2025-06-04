using Vge.Entity;
using Vge.Games;
using Vge.World;
using Vge.World.Block;

namespace Mvk2.World
{
    /// <summary>
    /// Объект миров этот объект отвечает за взаимосвязь всех миров на сервере.
    /// Для Малювек 2
    /// </summary>
    public class AllWorldsMvk : AllWorlds
    {
        int i1;
        int i2;

        public AllWorldsMvk()
        {
            _count = 2;
            _worldServers = new WorldServer[_count];
        }

        public override void Init(GameServer server)
        {
            base.Init(server);
            BlocksReg.Correct(server.Settings.TableBlocks);
            ModelEntitiesReg.Correct(server.Settings.TableEntities);

            // Инициализация ID сущностей и прочего
            //Server.ModServer.InitAfterStartGame();
            _InitAfterStartGame();

            _worldServers[0] = new WorldServer(server, 0, new WorldSettingsNightmare());
            _worldServers[1] = new WorldServer(server, 1, new WorldSettings());
        }

        /// <summary>
        /// Инициализация после старта игры, когда уже все блоки и сущности загружены
        /// </summary>
        private void _InitAfterStartGame()
        {
            int count = Ce.ModelEntities.Count;
            string s;
            for (int id = 0; id < count; id++)
            {
                s = Ce.ModelEntities.ModelEntitiesAlias[id];
                if (s == "Robinson") i1 = id;
                else if (s == "Chicken") i2 = id;
            }
            return;
        }

        /// <summary>
        /// Получить индекс кидаемого блока
        /// </summary>
        public override int GetDebugIndex(bool b) => b ? i1 : i2;
    }
}
