using System.Collections.Generic;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Карта чанков, через регион 5bit, а дальше массив
    /// </summary>
    public class MapChunk
    {
        private readonly Dictionary<long, Region> _map = new Dictionary<long, Region>();
        private readonly List<IChunkPosition> _list = new List<IChunkPosition>();

        /// <summary>
        /// Добавить или изменить
        /// </summary>
        public void Set(IChunkPosition value)
        {
            int x = value.CurrentChunkX;
            int y = value.CurrentChunkY;
            Remove(x, y);
            long xy = _Index(x, y);
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
                long xy = _Index(x, y);
                _map[xy].Remove(x, y);
            }
        }

        /// <summary>
        /// Получить чанк с массива
        /// </summary>
        public IChunkPosition Get(int x, int y)
        {
            long xy = _Index(x, y);
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
            long xy = _Index(x, y);
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

        /// <summary>
        /// Индекс для региона
        /// </summary>
        private long _Index(int x, int y)
        {
            long x1 = ((long)x >> 5) << 32;
            long y1 = y >> 5;
            return x1 + y1;
        }

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
