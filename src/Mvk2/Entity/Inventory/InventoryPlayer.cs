using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Inventory;
using Vge.Item;

namespace Mvk2.Entity.Inventory
{
    /// <summary>
    /// Инвентарь игрока
    /// </summary>
    public class InventoryPlayer : InventoryList
    {
        /// <summary>
        /// Стак который используется в перемещении из слотов, образно он в указателе мыши
        /// </summary>
        public ItemStack StackAir { get; private set; }

        /// <summary>
        /// Количество ячеек для предметов рюкзака
        /// </summary>
        private readonly int _backpackCount = 25;

        public InventoryPlayer()
            // Первый слот одеждый это ячейка левой руки
            : base(8, 11) { }

        /// <summary>
        /// Инициализация общего количества
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitAllCount()
            => _allCount = _mainCount + _clothCount + _backpackCount;

        /// <summary>
        /// Клик по указанному слоту в инвентаре на сервере!
        /// isRight указывает флаг правый ли клик мыши
        /// 0-99 слот персонажа, а именно (slot 0-7 инвентарь, 8-11 броня),
        /// 100-254 слот у блока с TileEntity по позиции
        /// 255 слот предмет в воздухе StackAir, быть не должен!
        /// </summary>
        public void ClickInventoryOnServer(int slotIn, bool isRight, bool isShift)
        {
            // TODO::2025-09-22 !!!
            return;
        }


        #region Event

        /// <summary>
        /// Событие изменён слот
        /// </summary>
        public event SlotEventHandler SlotChanged;
        /// <summary>
        /// Событие изменён слот
        /// </summary>
        protected override void _OnSlotChanged(int indexSlot)
            => SlotChanged?.Invoke(this, new SlotEventArgs(indexSlot));

        /// <summary>
        /// Событие изменён индекс выбраного слота правой руки
        /// </summary>
        public event EventHandler CurrentIndexChanged;
        /// <summary>
        /// Событие изменён индекс выбраного слота правой руки
        /// </summary>
        protected override void _OnCurrentIndexChanged()
            => CurrentIndexChanged?.Invoke(this, new EventArgs());

        #endregion
    }
}
