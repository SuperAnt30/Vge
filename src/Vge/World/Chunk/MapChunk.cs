using System;
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
        /// Общее количество элементов
        /// </summary>
        public int Count { get; private set; } = 0;

        /// <summary>
        /// Оптимизированный поиск чанка, служит для быстрого поиска чанка
        /// </summary>
        private readonly Dictionary<ulong, Region> _map = new Dictionary<ulong, Region>();

        /// <summary>
        /// Перерасчитать количество
        /// </summary>
        public void RecalculateCount()
        {
            Count = 0;
            foreach (Region region in _map.Values)
            {
                Count += region.Count;
            }
        }

        /// <summary>
        /// Получить список всех чанков
        /// </summary>
        public List<IChunkPosition> GetList()
        {
            List<IChunkPosition> chunks = new List<IChunkPosition>();

            foreach (Region region in _map.Values)
            {
                region.AddRegionList(chunks);
            }

            return chunks;
        }

        /// <summary>
        /// Cгенерировать список ключей всех регионов
        /// </summary>
        public List<ulong> GetRegionList()
        {
            List<ulong> list = new List<ulong>();
            foreach (ulong key in _map.Keys)
            {
                list.Add(key);
            }
            return list;
        }

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
            if (_map[xy].Set(x, y, value))
            {
                Count++;
            }
        }

        /// <summary>
        /// Удалить
        /// </summary>
        public void Remove(int x, int y)
        {
            IChunkPosition chunk = Get(x, y);
            if (chunk != null)
            {
                ulong xy = ((ulong)((uint)x >> 5) << 32) | ((uint)y >> 5);
                if (_map[xy].Remove(x, y))
                {
                    Count--;
                    if (_map[xy].Count == 0)
                    {
                        _map.Remove(xy);
                    }
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
                return _map[xy].Contains(x, y);
            }
            return false;
        }

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            _map.Clear();
            Count = 0;
        }

        /// <summary>
        /// Получить количество регионов
        /// </summary>
        public int RegionCount => _map.Count;

        /// <summary>
        /// Сгенерировать массив для отладки
        /// </summary>
        public IChunkPosition[] ToArrayDebug()
        {
            IChunkPosition[] destinationArray = new IChunkPosition[Count];
            int destinationIndex = 0;
            foreach (Region region in _map.Values)
            {
                Array.Copy(region.ToArrayDebug(), 0, destinationArray, destinationIndex, region.Count);
                destinationIndex += region.Count;
            }
            return destinationArray;
        }

        public override string ToString() => Count + "|" + RegionCount;

        /// <summary>
        /// Отдельные объект с массивом 1024 элемента
        /// </summary>
        private class Region
        {
            /// <summary>
            /// Количество значений
            /// </summary>
            public int Count { get; private set; } = 0;

            private readonly IChunkPosition[] _buffer = new IChunkPosition[1024];

            /// <summary>
            /// Проверить наличие чанка
            /// </summary>
            public bool Contains(int x, int y) => _buffer[(y & 31) << 5 | (x & 31)] != null;

            /// <summary>
            /// Получить чанк
            /// </summary>
            public IChunkPosition Get(int x, int y) => _buffer[(y & 31) << 5 | (x & 31)];

            /// <summary>
            /// Добавить чанк, вернёт true если новый
            /// </summary>
            public bool Set(int x, int y, IChunkPosition value)
            {
                int index = (y & 31) << 5 | (x & 31);
                if (_buffer[index] == null)
                {
                    // Создаём
                    Count++;
                    _buffer[index] = value;
                    return true;
                }
                // Перезаписываем
                _buffer[index] = value;
                return false;
            }

            /// <summary>
            /// Удалить, вернём true если было удаление
            /// </summary>
            public bool Remove(int x, int y)
            {
                int index = (y & 31) << 5 | (x & 31);
                if (_buffer[index] != null)
                {
                    // Удаляем
                    _buffer[index] = null;
                    Count--;
                    return true;
                }
                // Нечего удалять
                return false;
            }

            /// <summary>
            /// Добавить в лист присутствующие чанки
            /// </summary>
            public void AddRegionList(List<IChunkPosition> chunks)
            {
                for (int i = 0; i < 1024; i++)
                {
                    if (_buffer[i] != null) chunks.Add(_buffer[i]);
                }
            }

            /// <summary>
            /// Сгенерировать массив для отладки
            /// </summary>
            public IChunkPosition[] ToArrayDebug()
            {
                IChunkPosition[] ar = new IChunkPosition[Count];
                int index = 0;
                for (int i = 0; i < 1024; i++)
                {
                    if (_buffer[i] != null) ar[index++] = _buffer[i];
                }
                return ar;
            }

            public override string ToString() => Count.ToString();
        }
    }
}
