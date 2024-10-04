using Vge.World;
using Vge.World.Chunk;

namespace Vge.Management
{
    /// <summary>
    /// Объект управляет всеми чанками которые надо загрузить или выгрузить
    /// </summary>
    public class FragmentManager
    {
        /// <summary>
        /// Сетевой мир
        /// </summary>
        public readonly WorldServer World;

        private MapChunk map = new MapChunk();
        //private ChunkForAnchor[] list = new ChunkForAnchor[1024 * 32];

        public FragmentManager(WorldServer world)
        {
            World = world;

            World.Server.Filer.EndSectionLog();
            World.Server.Filer.StartSection("SetChunkForAnchor" + World.IdWorld);
            if (World.IdWorld == 1)
            {
                for (int x = -63; x < 65; x++)
                {
                    for (int y = -63; y < 65; y++)
                    {
                        map.Set(new ChunkForAnchor(World, x, y));
                    }
                }
            }
            else
            {
                for (int x = -200; x < 200; x++)
                {
                    for (int y = -200; y < 200; y++)
                    {
                        map.Set(new ChunkForAnchor(World, x, y));
                    }
                }
            }
            World.Server.Log.Server(World.IdWorld + " Count: {0}", map.Count);
            map.Remove(0, 0);
            map.Set(new ChunkForAnchor(World, 1, 1));
            World.Server.Filer.EndSectionLog();

            World.Server.Filer.StartSection("GetChunkForAnchor" + World.IdWorld);
            int i = 0;
            for (int x = -63; x < 65; x++)
            {
                for (int y = -63; y < 65; y++)
                {
                    map.Get(x, y);
                    i++;
                }
            }
            World.Server.Log.Server(World.IdWorld + " CountGet: {0}", i);
            World.Server.Filer.EndSectionLog();
        }

        public void Update()
        {
            return;
        }
    }
}
