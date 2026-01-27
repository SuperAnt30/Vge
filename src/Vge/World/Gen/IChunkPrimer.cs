using Vge.World.Block;
using Vge.World.BlockEntity;

namespace Vge.World.Gen
{
    /// <summary>
    /// Индерфейс подготовительного чанка для генерации
    /// </summary>
    public interface IChunkPrimer
    {
        void SetBlockState(int xz, int y, int id);

        void SetBlockState(int xz, int y, int id, int met);

        void SetBlockIdFlag(int xz, int y, int id, byte flag);

        void SetBlockStateTick(int xz, int y, int id, int met, uint tick);

        int GetBlockId(int xz, int y);

        /// <summary>
        /// Задать блок сущностей к конкретному блоку, пример дерево
        /// </summary>
        void SetBlockEntity(BlockEntityBase blockEntity);
        
        /// <summary>
        /// Получить блок сущности если имеется
        /// </summary>
        BlockEntityBase GetBlockEntity(int x, int y, int z);

        /// <summary>
        /// Очистить буфер
        /// </summary>
        void Clear();
    }
}
