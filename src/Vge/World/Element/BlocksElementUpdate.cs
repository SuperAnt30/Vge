using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Block;

namespace Vge.World.Element
{
    /// <summary>
    /// Базовый класс изменения блоков, для элементов, структур и прочего, в обновлении мира.
    /// Для роста деревьев и прочего, где взаимодействие множества блоков.
    /// </summary>
    public class BlocksElementUpdate
    {
        /// <summary>
        /// Массив кеш блоков для обновления структур текущего мира в потоке тиков
        /// </summary>
        public readonly ArrayFast<BlockCache> BlockCaches = new ArrayFast<BlockCache>(16384);

        /// <summary>
        /// Массив всех элементов генерации
        /// </summary>
        protected IElementUpdate[] _elements;

        /// <summary>
        /// Объект генерации элемента
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IElementUpdate Element(int index) => _elements[index];
    }
}
