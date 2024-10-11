namespace Vge.Management
{
    /// <summary>
    /// Интерфейс для якоря который может загружать чанки
    /// </summary>
    public interface IAnchor
    {
        /// <summary>
        /// Является ли якорь активным
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
        /// Координата Y в каком чанке находится
        /// </summary>
        int ChunkPositionY { get; }

        /// <summary>
        ///  В какой позиции X чанка было обработка видимых чанков
        /// </summary>
        int ChunkPosManagedX { get; }
        /// <summary>
        ///  В какой позиции Y чанка было обработка видимых чанков
        /// </summary>
        int ChunkPosManagedY { get; }

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
        /// <param name="chunkPosY">Позиция Y чанка</param>
        void AddChunk(int chunkPosX, int chunkPosY);
        /// <summary>
        /// Удалить якорь из конкретного чанка
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosY">Позиция Y чанка</param>
        void RemoveChunk(int chunkPosX, int chunkPosY);

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
