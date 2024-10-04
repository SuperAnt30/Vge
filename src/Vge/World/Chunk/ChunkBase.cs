namespace Vge.World.Chunk
{
    /// <summary>
    /// Базовый объект чанка
    /// </summary>
    public class ChunkBase
    {
        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public readonly int CurrentChunkX;
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public readonly int CurrentChunkY;
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public readonly WorldBase World;
        /// <summary>
        /// Совокупное количество тиков, которые якори провели в этом чанке 
        /// </summary>
        public uint InhabitedTakt { get; private set; }

        public ChunkBase(WorldBase world, int chunkPosX, int chunkPosY)
        {
            World = world;
            CurrentChunkX = chunkPosX;
            CurrentChunkY = chunkPosY;
        }

        /// <summary>
        /// Задать совокупное количество тактов, которые якоря провели в этом чанке 
        /// </summary>
        public void SetInhabitedTime(uint takt) => InhabitedTakt = takt;
    }
}
