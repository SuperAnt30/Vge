using Vge.World;

namespace Mvk2.World
{
    public class AllWorldsMvk : AllWorlds
    {
        public AllWorldsMvk()
        {
            _count = 2;
            _worldServers = new WorldServer[_count];
        }
    }
}
