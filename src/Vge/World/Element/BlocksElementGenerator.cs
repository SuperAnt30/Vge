using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Block;

namespace Vge.World.Element
{
    /// <summary>
    /// Базовый класс генерации блоков, для элементов, структур и прочего, не генерации чанка
    /// Для роста деревьев и прочего, где взаимодействие множества блоков.
    /// Т.е. когда уже мир сгенерирован, и делаем доп генерации
    /// </summary>
    public class BlocksElementGenerator
    {
        /// <summary>
        /// Массив кеш блоков для генерации структур текущего мира
        /// </summary>
        public readonly ArrayFast<BlockCache> BlockCaches = new ArrayFast<BlockCache>(16384);

        /// <summary>
        /// Массив всех элементов генерации
        /// </summary>
        protected IElementGenerator[] _elements;

        /// <summary>
        /// Объект генерации элемента
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IElementGenerator Element(int index) => _elements[index];
    }
}
