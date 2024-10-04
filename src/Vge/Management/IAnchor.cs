namespace Vge.Management
{
    /// <summary>
    /// Интерфейс для якоря который может загружать чанки
    /// </summary>
    public interface IAnchor
    {

        /// <summary>
        /// Добавить якорь в конкретный чанк
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosY">Позиция Y чанка</param>
        /// <param name="isLoaded">Нужно ли добавить в список загруженых чанков, надо для спавна якоря</param>
        void AddChunk(int chunkPosX, int chunkPosY, bool isLoaded);

    }
}
