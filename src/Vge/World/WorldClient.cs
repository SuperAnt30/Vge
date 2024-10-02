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
            Filer = new Profiler(game.Log, "[Client] ");
        }
    }
}
