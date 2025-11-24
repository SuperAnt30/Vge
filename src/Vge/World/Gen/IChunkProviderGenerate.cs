using Vge.Games;
using Vge.World.Chunk;

namespace Vge.World.Gen
{
    /// <summary>
    /// Интерфейс генерации чанка
    /// </summary>
    public interface IChunkProviderGenerate
    {
        /// <summary>
        /// Генерация рельефа чанка, соседние чанки не требуются
        /// </summary>
        void Relief(ChunkBase chunk);

        /// <summary>
        /// Декорация чанков, требуются соседние чанки (3*3)
        /// </summary>
        void Populate(ChunkBase chunk);
    }
}
