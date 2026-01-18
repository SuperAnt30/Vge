using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Vge.World.Gen
{
    /// <summary>
    /// Интерфейс генерации чанка
    /// </summary>
    public interface IChunkProviderGenerate
    {
        /// <summary>
        /// Массив кеш блоков для генерации структур текущего мира
        /// </summary>
        ArrayFast<BlockCache> BlockCaches { get; }

        /// <summary>
        /// Генерация рельефа чанка, соседние чанки не требуются
        /// </summary>
        void Relief(ChunkServer chunk);

        /// <summary>
        /// Декорация чанков, требуются соседние чанки (3*3)
        /// </summary>
        void Decoration(ChunkProviderServer provider, ChunkServer chunk);

        /// <summary>
        /// Объект генерации элемента
        /// </summary>
        IElementGenerator Element(string key);
    }
}
