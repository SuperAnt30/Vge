namespace Vge.Management
{
    /// <summary>
    /// Интерфейс для якоря который может загружать чанки
    /// </summary>
    public interface IAnchor
    {
        /// <summary>
        /// Является ли якорь активным, чтоб тикали рядом чанки
        /// </summary>
        bool IsActive { get; }
        /// <summary>
        /// Активный радиус обзора для сервера, нужен для спавна и тиков блоков
        /// </summary>
        byte ActiveRadius { get; }
        /// <summary>
        /// Является ли якорь игроком
        /// </summary>
        bool IsPlayer { get; }

        /// <summary>
        /// Обзор сколько видит якорь чанков вокруг себя
        /// </summary>
        byte OverviewChunk { get; }
        /// <summary>
        /// Обзор чанков прошлого такта
        /// </summary>
        byte OverviewChunkPrev { get; }

        /// <summary>
        /// Координату X в каком чанке находится
        /// </summary>
        int ChunkPositionX { get; }
        /// <summary>
        /// Координата Z в каком чанке находится
        /// </summary>
        int ChunkPositionZ { get; }

        /// <summary>
        ///  В какой позиции X чанка было обработка видимых чанков
        /// </summary>
        int ChunkPosManagedX { get; }
        /// <summary>
        ///  В какой позиции Z чанка было обработка видимых чанков
        /// </summary>
        int ChunkPosManagedZ { get; }

        /// <summary>
        /// Возвращает имеется ли хоть один чанк для загрузки
        /// </summary>
        bool CheckLoadingChunks();

        /// <summary>
        /// Вернуть координаты чанка для загрузки
        /// </summary>
        ulong ReturnChunkForLoading();

        /// <summary>
        /// Добавить якорь в конкретный чанк
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosZ">Позиция Z чанка</param>
        void AddChunk(int chunkPosX, int chunkPosZ);
        /// <summary>
        /// Удалить якорь из конкретного чанка
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosZ">Позиция Z чанка</param>
        void RemoveChunk(int chunkPosX, int chunkPosZ);

        /// <summary>
        /// Установленный перемещенный якорь
        /// </summary>
        void MountedMovedAnchor();

        /// <summary>
        /// Изменение обзора,
        /// </summary>
        bool IsChangeOverview();
        
        /// <summary>
        /// Необходимо ли смещение?
        /// </summary>
        bool IsAnOffsetNecessary();
    }
}
