using System;
using System.Runtime.CompilerServices;
using Vge.Item;
using Vge.NBT;
using Vge.World;

namespace Vge.Entity.Inventory
{
    /// <summary>
    /// Базовый класс инвенторя, пустой, без предметов
    /// </summary>
    public class InventoryBase
    {
        /// <summary>
        /// Общее количество видимых ячеек, не должно привышать 30!
        /// из-за битового флага, для обновлений
        /// </summary>
        public int OutsideCount { get; protected set; } = 0;

        /// <summary>
        /// Обновление на сервере каждый тик
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void UpdateServer(EntityBase entity, WorldServer worldServer) { }

        /// <summary>
        /// Получить выбранный слот правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual byte GetCurrentIndex() => 0;

        /// <summary>
        /// Задать активный слот правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool SetCurrentIndex(byte slotIn) => false;

        /// <summary>
        /// Сместить слот правой руки в большую сторону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SlotMore() { }

        /// <summary>
        /// Сместить слот правой руки в меньшую сторону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SlotLess() { }

        /// <summary>
        /// Получить текущий стак правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ItemStack GetCurrentItem() => null;

        /// <summary>
        /// Задать в текущий стак правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetCurrentItem(ItemStack stack) { }

        /// <summary>
        /// Возвращает стaк в слоте slotIn
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ItemStack GetStackInSlot(int slotIn) => null;

        /// <summary>
        /// Устанавливает стaк в слоте slotIn, без событий!!!, для загрузки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetStackInSlot(int slotIn, ItemStack stack) { }

        /// <summary>
        /// Устанавливает данный стек предметов в указанный слот в инвентаре
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetInventorySlotContents(int slotIn, ItemStack stack) { }

        /// <summary>
        /// Получить стак по слоту внешности (что в руках и одежда)
        /// для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ItemStack GetOutside(int slotIn) => null;

        /// <summary>
        /// Получить список стаков для внешности (что в руках и одежда)
        /// для передачи по сети
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ItemStack[] GetOutside() => new ItemStack[0];

        /// <summary>
        /// Задать список стаков для внешности (что в руках и одежда)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetOutside(ItemStack[] stacks) { }
        
        /*
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
        */

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

        /// <summary>
        /// Изменение удерживаемого предмета на сервере. Предмет в руке сейчас будет изменён
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void ServerHeldItemChange() { }

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
                SetStackInSlot(slot.Index, slot.Stack);
            }
        }

        #region Event

        /// <summary>
        /// Событие изменён слот
        /// </summary>
        protected virtual void _OnSlotChanged(int indexSlot) { }

        /// <summary>
        /// Событие изменён индекс выбраного слота правой руки
        /// </summary>
        protected virtual void _OnCurrentIndexChanged() { }

        /// <summary>
        /// Событие изменён предмет внешности (что в руках или одежда)
        /// </summary>
        public event EventHandler OutsideChanged;
        /// <summary>
        /// Событие изменён предмет внешности (что в руках или одежда)
        /// flags = -1 все
        /// </summary>
        protected virtual void _OnOutsideChanged(int flags)
            => OutsideChanged?.Invoke(this, new EventArgs());

        #endregion

    }
}
