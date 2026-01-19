using Vge.World.Block;

namespace Vge.World.Element
{
    /// <summary>
    /// Интерфейс генерации элемента
    /// </summary>
    public interface IElementGenerator
    {
        /// <summary>
        /// Генерация элемента в выбранной позиции
        /// </summary>
        void Generation(WorldServer world, BlockPos blockPos);
    }
}
