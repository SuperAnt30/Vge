using Vge.Util;

namespace Vge.Item
{
    /// <summary>
    /// Таблица блоков для регистрации
    /// </summary>
    public class ItemRegTable : RegTable<ItemBase>
    {
        /// <summary>
        /// Создать пустой предмет
        /// </summary>
        protected override ItemBase _CreateNull() => new ItemBase();
    }
}
