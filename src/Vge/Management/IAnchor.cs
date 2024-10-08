using System.Collections.Generic;
using Vge.Util;

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
