using System.Collections.Generic;
using Vge.Management;
using Vge.Util;

namespace Vge.World
{
    /// <summary>
    /// Мировой якорь, для временных чанков
    /// </summary>
    public class WorldAnchor : IAnchor
    {
        #region Anchor

        /// <summary>
        /// Является ли якорь активным
        /// </summary>
        public bool IsActive => false;
        /// <summary>
        /// Обзор сколько видит якорь чанков вокруг себя
        /// </summary>
        public byte OverviewChunk => 0;
        /// <summary>
        /// Обзор чанков прошлого такта
        /// </summary>
        public byte OverviewChunkPrev => 0;
        /// <summary>
        /// Координата X в каком чанке находится
        /// </summary>
        public int ChunkPositionX { get; private set; }
        /// <summary>
        /// Координата Y в каком чанке находится
        /// </summary>
        public int ChunkPositionY { get; private set; }
        /// <summary>
        /// В какой позиции X чанка было обработка видимых чанков
        /// </summary>
        public int ChunkPosManagedX => ChunkPositionX;
        /// <summary>
        /// В какой позиции Y чанка было обработка видимых чанков
        /// </summary>
        public int ChunkPosManagedY => ChunkPositionY;
        /// <summary>
        /// Список чанков нужно проверить на загрузку или генерацию,
        /// должен формироваться по дистанции от игрока.
        /// </summary>
        public ListFast<ulong> LoadingChunks { get; private set; } = new ListFast<ulong>(1);

        #endregion

        /// <summary>
        /// Счётчик жизни
        /// </summary>
        private int _counterLife;

        /// <summary>
        /// Посредник серверного чанка
        /// </summary>
        private readonly WorldServer _worldServer;

        public WorldAnchor(WorldServer world, int x, int y)
        {
            ChunkPositionX = x;
            ChunkPositionY = y;
            _worldServer = world;
            LoadingChunks.Add(Conv.ChunkXyToIndex(x, y));
        }

        /// <summary>
        /// Продлить жизнь
        /// </summary>
        public void ProlongLife() => _counterLife = 0;

        /// <summary>
        /// Обновление и проверка на удаление, true вернёт, значит удалить можно
        /// </summary>
        public bool UpdateAndCheck()
        {
            // Жизнь 10 секунд
            return ++_counterLife > 3;
        }

        #region Anchor

        /// <summary>
        /// Добавить якорь в конкретный чанк
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosY">Позиция Y чанка</param>
        public void AddChunk(int chunkPosX, int chunkPosY) { }
        /// <summary>
        /// Удалить якорь из конкретного чанка
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosY">Позиция Y чанка</param>
        public void RemoveChunk(int chunkPosX, int chunkPosY) { }

        /// <summary>
        /// Установленный перемещенный якорь
        /// </summary>
        public void MountedMovedAnchor() { }

        /// <summary>
        /// Изменение обзора,
        /// </summary>
        public bool IsChangeOverview() => false;

        /// <summary>
        /// Необходимо ли смещение?
        /// </summary>
        public bool IsAnOffsetNecessary() => false;

        #endregion
    }
}
