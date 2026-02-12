using System.Collections.Generic;

namespace Vge.World
{
    /// <summary>
    /// Объект который хранит и отвечает за кэш регионов
    /// </summary>
    public class RegionProvider
    {
        // ulong xy = ((ulong)((uint)x >> 5) << 32) | ((uint)y >> 5);
        // new Vector2i((int)((xy & 0xFFFFFFFF00000000) >> 27), (int)xy << 5);

        /// <summary>
        /// Список регионов
        /// </summary>
        private Dictionary<ulong, RegionFile> _map = new Dictionary<ulong, RegionFile>();

        /// <summary>
        /// Сылка на объект мира сервера
        /// </summary>
        private readonly WorldServer _world;

        private readonly object locker = new object();

        public RegionProvider(WorldServer world)
        {
            _world = world;
        }

        /// <summary>
        /// Получить Файл региона по его координатам чанка
        /// </summary>
        public RegionFile Get(int chunkX, int chunkY)
        {
            int x = chunkX >> 5;
            int y = chunkY >> 5;
            ulong xy = ((ulong)((uint)x) << 32) | ((uint)y);
            if (!_map.ContainsKey(xy))
            {
                lock (locker)
                {
                    RegionFile region = new RegionFile(_world, x, y);
                    _map.Add(xy, region);
                }
            }
            return _map[xy];
        }

        /// <summary>
        /// Получить количество регионов
        /// </summary>
        public int RegionCount => _map.Count;

        /// <summary>
        /// Сохранение регионов
        /// </summary>
        public void WriteToFile(bool isStoping)
        {
            lock (locker)
            {
                foreach (RegionFile region in _map.Values)
                {
                    region.WriteToFile();
                }
            }
            if (!isStoping)
            {
                // Проверяем какие регионы можно выгрузить
                List<ulong> list = _world.ChunkPrServ.GetRegionList();
                List<ulong> listRemove = new List<ulong>();
                foreach (ulong key in _map.Keys)
                {
                    if (!list.Contains(key))
                    {
                        listRemove.Add(key);
                    }
                }
                foreach (ulong key in listRemove)
                {
                    _map.Remove(key);
                }
            }
        }
    }
}
