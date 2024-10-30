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
        /// Общекк количество элементов
        /// </summary>
        public int Count { get; private set; } = 0;

        /// <summary>
        /// Оптимизированный поиск чанка, служит для быстрого поиска чанка
        /// </summary>
        private readonly Dictionary<ulong, Region> _map = new Dictionary<ulong, Region>();

        private int _x, _y;
        private ulong _xy;

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
        /// Добавить
        /// </summary>
        public void Add(IChunkPosition value)
        {
            _x = value.CurrentChunkX;
            _y = value.CurrentChunkY;
            _xy = ((ulong)((uint)_x >> 5) << 32) | ((uint)_y >> 5);
            if (!_map.ContainsKey(_xy))
            {
                _map.Add(_xy, new Region());
            }
            if (_map[_xy].Set(_x, _y, value))
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
                _xy = ((ulong)((uint)x >> 5) << 32) | ((uint)y >> 5);
                if (_map[_xy].Remove(x, y))
                {
                    Count--;
                    if (_map[_xy].Count == 0)
                    {
                        _map.Remove(_xy);
                    }
                }
            }
        }

        /// <summary>
        /// Получить чанк с массива
        /// </summary>
        public IChunkPosition Get(int x, int y)
        {
            _xy = ((ulong)((uint)x >> 5) << 32) | ((uint)y >> 5);
            if (_map.ContainsKey(_xy))
            {
                return _map[_xy].Get(x, y);
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
            _xy = ((ulong)((uint)x >> 5) << 32) | ((uint)y >> 5);
            if (_map.ContainsKey(_xy))
            {
                return _map[_xy].Contains(x, y);
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

            private int _index;

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
                _index = (y & 31) << 5 | (x & 31);
                if (_buffer[_index] == null)
                {
                    // Создаём
                    Count++;
                    _buffer[_index] = value;
                    return true;
                }
                // Перезаписываем
                _buffer[_index] = value;
                return false;
            }

            /// <summary>
            /// Удалить, вернём true если было удаление
            /// </summary>
            public bool Remove(int x, int y)
            {
                _index = (y & 31) << 5 | (x & 31);
                if (_buffer[_index] != null)
                {
                    // Удаляем
                    _buffer[_index] = null;
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
                _index = 0;
                for (int i = 0; i < 1024; i++)
                {
                    if (_buffer[i] != null) ar[_index++] = _buffer[i];
                }
                return ar;
            }

            public override string ToString() => Count.ToString();
        }
    }
}
