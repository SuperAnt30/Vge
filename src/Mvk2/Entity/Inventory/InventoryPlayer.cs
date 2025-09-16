using System;
using Vge.Entity.Inventory;

namespace Mvk2.Entity.Inventory
{
    /// <summary>
    /// Инвентарь игрока
    /// </summary>
    public class InventoryPlayer : InventoryList
    {
        /// <summary>
        /// Количество ячеек для предметов рюкзака
        /// </summary>
        private readonly int _backpackCount;

        public InventoryPlayer()
            // Первый слот одеждый это ячейка левой руки
            : base(8, 11) 
        {
            _backpackCount = 25;
            _allCount += _backpackCount;
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
