namespace Vge.World.Chunk
{
    public interface IChunkPosition
    {
        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        int CurrentChunkX { get; }
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        int CurrentChunkY { get; }
        /// <summary>
        /// Дополнительные данные
        /// </summary>
        object Tag { get; }
    }
}
