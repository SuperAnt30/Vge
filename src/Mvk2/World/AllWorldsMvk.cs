using Mvk2.World.Block;
using Vge.Entity;
using Vge.Games;
using Vge.World;
using Vge.World.Block;

namespace Mvk2.World
{
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
            BlocksReg.Correct(server.Settings.TableBlocks);
            ModelEntitiesReg.Correct(server.Settings.TableEntities);

            //Blocks.InitializationAfterItems();

            _worldServers[0] = new WorldServer(server, 0, new WorldSettingsNightmare());
            _worldServers[1] = new WorldServer(server, 1, new WorldSettings());
        }
    }
}
