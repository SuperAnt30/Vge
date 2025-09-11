using System.Runtime.CompilerServices;
using Vge.Item;
using Vge.NBT;

namespace Vge.Entity.Inventory
{
    /// <summary>
    /// Базовый класс инвенторя, пустой, без предметов
    /// </summary>
    public class InventoryBase
    {
        /// <summary>
        /// Получить выбранный слот правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual byte GetCurrentIndex() => 0;

        /// <summary>
        /// Задать активный слот быстрого доступа
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool SetCurrentIndex(byte slotIn) => false;

        /// <summary>
        /// Сместить слот быстрого доступа в большую сторону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SlotMore() { }

        /// <summary>
        /// Сместить слот быстрого доступа в меньшую сторону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SlotLess() { }

        /// <summary>
        /// Возвращает стaк в слоте slotIn
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ItemStack GetStackInSlot(int slotIn) => null;

        /// <summary>
        /// Устанавливает данный стек предметов в указанный слот в инвентаре
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetInventorySlotContents(int slotIn, ItemStack stack) { }

        /// <summary>
        /// Получить список стаков (что в руке и список одежды)
        /// для передачи по сети
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ItemStack[] GetCurrentItemAndCloth() => new ItemStack[0];

        /// <summary>
        /// Задать список стаков (что в руке и список одежды)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetCurrentItemAndCloth(ItemStack[] stacks) { }

        /// <summary>
        /// Получить стак одежды
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ItemStack GetClothInventory(int slotIn) => null;

        /// <summary>
        /// Задать стак одежды
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetClothInventory(int slotIn, ItemStack stack) { }

        /// <summary>
        /// Получить выбранный стак правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ItemStack GetCurrentItem() => null;

        /// <summary>
        /// Задать в правую руку стак
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetCurrentItem(ItemStack stack) { }

        /// <summary>
        /// Получить полный список всего инвентаря
        /// Mvk было GetMainAndCloth
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ItemStack[] GetAll() => new ItemStack[0];

        /// <summary>
        /// Задать полный список всего инвентаря
        /// Mvk было SetMainAndCloth
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetAll(ItemStack[] stacks) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _Clear() { }

        public virtual void WriteToNBT(TagCompound nbt)
            => NBTTools.ItemStacksWriteToNBT(nbt, "Inventory", GetAll());

        public virtual void ReadFromNBT(TagCompound nbt)
        {
            _Clear();
            Slot[] slots = NBTTools.ItemStacksReadFromNBT(nbt, "Inventory");
            foreach (Slot slot in slots)
            {
                SetInventorySlotContents(slot.Index, slot.Stack);
            }
        }

        #region Event

        /// <summary>
        /// Событие изменён слот из инвентаря или одежды или рюкзака
        /// </summary>
        protected virtual void _OnChanged(int indexSlot) { }

        /// <summary>
        /// Событие изменён предмет в руке или выбраный слот правой руки
        /// </summary>
        protected virtual void _OnCurrentItemChanged() { }

        #endregion

    }
}
