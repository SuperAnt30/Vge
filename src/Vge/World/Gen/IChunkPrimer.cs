using Vge.World.Block;
using Vge.World.BlockEntity;

namespace Vge.World.Gen
{
    /// <summary>
    /// Индерфейс подготовительного чанка для генерации
    /// </summary>
    public interface IChunkPrimer
    {
        /// <summary>
        /// Задать блок
        /// </summary>
        /// <param name="xz">z << 4 | x</param>
        void SetBlockState(int xz, int y, int id);

        /// <summary>
        /// Задать блок с метданными
        /// </summary>
        /// <param name="xz">z << 4 | x</param>
        void SetBlockState(int xz, int y, int id, int met);

        /// <summary>
        /// Задать блок с флагом,
        /// 0 = всегда меняем
        /// 1 = меняем если не воздух
        /// 2 = меняем если только воздух
        /// </summary>
        /// <param name="xz">z << 4 | x</param>
        void SetBlockIdFlag(int xz, int y, int id, byte flag);

        /// <summary>
        /// Задать блок кеша
        /// </summary>
        void SetBlockCache(BlockCache blockCache);

        /// <summary>
        /// Получить id блока
        /// </summary>
        /// <param name="xz">z << 4 | x</param>
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
