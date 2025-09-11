namespace Vge.Item
{
    /// <summary>
    /// Различные массивы предметов
    /// </summary>
    public sealed class ItemArrays
    {
        /// <summary>
        /// Массив названий предметов
        /// </summary>
        public readonly string[] ItemAlias;
        /// <summary>
        /// Массив объектов предметов
        /// </summary>
        public readonly ItemBase[] ItemObjects;
        /// <summary>
        /// Количество всех предметов
        /// </summary>
        public readonly int Count;

        public ItemArrays()
        {
            Count = ItemsReg.Table.Count;
            ItemAlias = new string[Count];
            ItemObjects = new ItemBase[Count];

            ItemBase item;
            for (ushort id = 0; id < Count; id++)
            {
                ItemAlias[id] = ItemsReg.Table.GetAlias(id);
                item = ItemsReg.Table[id];
                item.SetIndex(id);
                ItemObjects[id] = item;
            }
        }
    }
}
