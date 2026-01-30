using Vge.Games;
using Vge.World;

namespace Mvk2.World
{
    /// <summary>
    /// Объект миров этот объект отвечает за взаимосвязь всех миров на сервере.
    /// Для Малювек 2
    /// </summary>
    public class AllWorldsMvk : AllWorlds
    {

        public AllWorldsMvk()
        {
            _count = 2;
            _worldServers = new WorldServer[_count];
        }

        public override void Init(GameServer server)
        {
            base.Init(server);

            // Корректировка блоков, сущностей и прочего перед инициализации миров
            Server.ModServer.CorrectObjects();
            
            _worldServers[0] = new WorldServer(server, 0, new WorldSettingsIsland(server.Settings.Seed));
            _worldServers[1] = new WorldServer(server, 1, new WorldSettingsNightmare());

            // Дополнительная инициализация блоков после инициализации миров
            Ce.Blocks.InitializationAfterItemsN3();
        }
    }
}
