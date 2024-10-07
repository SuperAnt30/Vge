using System.Collections.Generic;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Карта чанков, через регион 5bit, а дальше массив
    /// </summary>
    public class MapChunk
    {
        // ulong xy = ((ulong)((uint)x >> 5) << 32) | ((uint)y >> 5);
        // new Vector2i((int)((xy & 0xFFFFFFFF00000000) >> 27), (int)xy << 5);

        /// <summary>
        /// Оптимизированный поиск чанка, служит для быстрого поиска чанка
        /// </summary>
        private readonly Dictionary<ulong, Region> _map = new Dictionary<ulong, Region>();
        /// <summary>
        /// Список всех чанков, служит для получения полного списка
        /// </summary>
        private readonly List<IChunkPosition> _list = new List<IChunkPosition>();
        
        /// <summary>
        /// Добавить
        /// </summary>
        public void Add(IChunkPosition value)
        {
            int x = value.CurrentChunkX;
            int y = value.CurrentChunkY;
            ulong xy = ((ulong)((uint)x >> 5) << 32) | ((uint)y >> 5);
            if (!_map.ContainsKey(xy))
            {
                _map.Add(xy, new Region());
            }
            _map[xy].Set(x, y, value);
            _list.Add(value);
        }

        /// <summary>
        /// Удалить
        /// </summary>
        public void Remove(int x, int y)
        {
            IChunkPosition chunk = Get(x, y);
            if (chunk != null)
            {
                _list.Remove(chunk);
                ulong xy = ((ulong)((uint)x >> 5) << 32) | ((uint)y >> 5);
                if (_map[xy].Remove(x, y))
                {
                    _map.Remove(xy);
                }
            }
        }

        /// <summary>
        /// Получить чанк с массива
        /// </summary>
        public IChunkPosition Get(int x, int y)
        {
            ulong xy = ((ulong)((uint)x >> 5) << 32) | ((uint)y >> 5);
            if (_map.ContainsKey(xy))
            {
                return _map[xy].Get(x, y);
            }
            return null;
        }

        /// <summary>
        /// Проверить наличие чанка
        /// </summary>
        public bool Contains(IChunkPosition chunk)
            => Contains(chunk.CurrentChunkX, chunk.CurrentChunkY);

        /// <summary>
        /// Проверить наличие чанка
        /// </summary>
        public bool Contains(int x, int y)
        {
            ulong xy = ((ulong)((uint)x >> 5) << 32) | ((uint)y >> 5);
            if (_map.ContainsKey(xy))
            {
                return _map[xy].Get(x, y) != null;
            }
            return false;
        }

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            _map.Clear();
            _list.Clear();
        }

        /// <summary>
        /// Получить количество
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Получить коллекцию чанков
        /// </summary>
        public List<IChunkPosition> GetList() => _list;

        private class Region
        {
            private readonly IChunkPosition[] _ar = new IChunkPosition[1024];
            private int _count = 0;

            public IChunkPosition Get(int x, int y) => _ar[(y & 31) << 5 | (x & 31)];

            public void Set(int x, int y, IChunkPosition value)
            {
                int index = (y & 31) << 5 | (x & 31);
                if (_ar[index] == null) _count++;
                _ar[index] = value;
            }
            /// <summary>
            /// Удалить, вернём true если ничего не осталось
            /// </summary>
            public bool Remove(int x, int y)
            {
                int index = (y & 31) << 5 | (x & 31);
                if (_ar[index] != null)
                {
                    _ar[index] = null;
                    _count--;
                }
                if (_count <= 0) return true;
                return false;
            }

            public override string ToString() => _count.ToString();
        }
    }
}
