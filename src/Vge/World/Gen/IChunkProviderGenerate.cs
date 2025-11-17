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
        /// Инициализировать при загрузке мира
        /// </summary>
        void InitLoading(GameServer server, WorldServer worldServer);

        /// <summary>
        /// Генерация чанка
        /// </summary>
        void GenerateChunk(ChunkBase chunk);

        /// <summary>
        /// Декорация чанков, требуются соседние чанки (3*3)
        /// </summary>
        void Populate(ChunkBase chunk);
    }
}
