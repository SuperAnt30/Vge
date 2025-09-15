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
            : base(9, 10) 
        {
            _backpackCount = 25;
            _allCount += _backpackCount;
        }

        #region Event

        /// <summary>
        /// Событие изменён слот из инвентаря или одежды или рюкзака
        /// </summary>
        public event SlotEventHandler Changed;
        /// <summary>
        /// Событие изменён слот из инвентаря или одежды или рюкзака
        /// </summary>
        protected override void _OnChanged(int indexSlot) 
            => Changed?.Invoke(this, new SlotEventArgs(indexSlot));

        /// <summary>
        /// Событие изменён предмет в руке или выбраный слот правой руки
        /// </summary>
        public event EventHandler CurrentItemChanged;
        /// <summary>
        /// Событие изменён предмет в руке или выбраный слот правой руки
        /// </summary>
        protected override void _OnCurrentItemChanged() 
            => CurrentItemChanged?.Invoke(this, new EventArgs());

        #endregion
    }
}
