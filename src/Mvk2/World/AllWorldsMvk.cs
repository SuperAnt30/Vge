using Mvk2.World.Block;
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
            BlocksRegMvk.Initialization();
            BlocksReg.Correct(server.Settings.Table);

            _worldServers[0] = new WorldServer(server, 0, new WorldSettingsNightmare());
            _worldServers[1] = new WorldServer(server, 1, new WorldSettings());
        }
    }
}
