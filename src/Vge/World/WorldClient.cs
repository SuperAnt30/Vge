using Vge.Games;
using Vge.Util;

namespace Vge.World
{
    /// <summary>
    /// Клиентский объект мира
    /// </summary>
    public class WorldClient : WorldBase
    {
        public WorldClient(GameBase game)
        {
            IsRemote = true;
            Rnd = new Rand();
            Filer = new Profiler(game.Log, "[Client] ");
        }
    }
}
