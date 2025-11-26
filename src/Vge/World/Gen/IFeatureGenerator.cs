using Vge.Util;
using Vge.World.Chunk;

namespace Vge.World.Gen
{
    /// <summary>
    /// Интерфейс генерации особенностей, элементов
    /// </summary>
    public interface IFeatureGeneratorColumn
    {
        /// <summary>
        /// Декорация блока или столба не выходящего за чанк
        /// </summary>
        void DecorationsColumn(ChunkBase chunkSpawn, Rand rand);
    }

    /// <summary>
    /// Интерфейс генерации особенностей, элементов
    /// </summary>
    public interface IFeatureGeneratorArea
    {
        /// <summary>
        /// Декорация областей которые могу выйти за 1 чанк
        /// </summary>
        void DecorationsArea(ChunkBase chunkSpawn, Rand rand, int biasX, int biasZ);
    }
}
