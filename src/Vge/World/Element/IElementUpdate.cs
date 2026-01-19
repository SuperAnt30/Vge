using Vge.World.Block;

namespace Vge.World.Element
{
    /// <summary>
    /// Интерфейс изменения элемента в такте мира
    /// </summary>
    public interface IElementUpdate
    {
        /// <summary>
        /// Изменение элемента в выбранной позиции
        /// </summary>
        void Update(WorldServer world, BlockPos blockPos);
    }
}
