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
            
            _worldServers[0] = new WorldServer(server, 0, 
                new WorldSettingsIsland(server.Settings.GetPathWorld(0), server));
            _worldServers[1] = new WorldServer(server, 1,
                new WorldSettingsNightmare(server.Settings.GetPathWorld(1)));

            // Дополнительная инициализация блоков после инициализации миров и корректировки id блоков
            Ce.Blocks.InitializationAfterItems();
        }
    }
}
