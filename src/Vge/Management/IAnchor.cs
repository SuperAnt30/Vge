using Vge.Util;

namespace Vge.Management
{
    /// <summary>
    /// Интерфейс для якоря который может загружать чанки
    /// </summary>
    public interface IAnchor
    {
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
        /// Список чанкоы нужно проверить на загрузку или генерацию,
        /// должен формироваться по дистанции от игрока.
        /// </summary>
        ListFast<ulong> LoadingChunks { get; }

        /// <summary>
        /// Добавить якорь в конкретный чанк
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosY">Позиция Y чанка</param>
        void AddChunk(int chunkPosX, int chunkPosY);

        /// <summary>
        /// Обновить обзор прошлого такта
        /// </summary>
        void UpOverviewChunkPrev();

        /// <summary>
        /// Обновить чанк обработки
        /// </summary>
        void UpChunkPosManaged();

        
    }
}
