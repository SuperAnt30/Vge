using Vge.Item;

namespace Vge.Entity.Inventory
{
    /// <summary>
    /// Слот инвентаря, для блоков и предметов
    /// </summary>
    public class Slot
    {
        /// <summary>
        /// Индекс слота
        /// </summary>
        public short Index { get; private set; }
        /// <summary>
        /// Объект одной ячейки
        /// </summary>
        public ItemStack Stack { get; private set; }

        /// <summary>
        /// Пустой слот без ячейки
        /// </summary>
        public Slot()
        {
            Index = -1;
            Stack = null;
        }

        /// <summary>
        /// В конкретный слот ячейки, либо имеется стак либо пустой
        /// </summary>
        public Slot(short index, ItemStack stack = null)
        {
            Index = index;
            Stack = stack;
        }

        /// <summary>
        /// Добавить стак в воздухе, т.е. в руке между слотами инвентаря
        /// </summary>
        public Slot(ItemStack stack)
        {
            Index = -2;
            Stack = stack;
        }

        /// <summary>
        /// Стак в воздухе, т.е. в руке между слотами инвентаря
        /// </summary>
        public bool IsAir() => Index == -2;

        public Slot Clone() => new Slot(Index, Stack?.Copy());

        public void Clear()
        {
            Index = -1;
            Stack = null;
        }

        /// <summary>
        /// Задать в текущий слот, стак
        /// </summary>
        public void Set(ItemStack stack = null) => Stack = stack;

        /// <summary>
        /// Задать конкретный слот
        /// </summary>
        public void SetIndex(short index) => Index = index;

        /// <summary>
        /// Задать стак в воздухе, т.е. в руке между слотами инвентаря
        /// </summary>
        public void SetAir() => Index = -2;

        /// <summary>
        /// Имеется ли стак
        /// </summary>
        public bool Empty() => Stack == null || Stack.Item == null;
    }
}
