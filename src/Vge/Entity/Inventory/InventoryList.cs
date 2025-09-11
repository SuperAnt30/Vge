using System;
using System.Runtime.CompilerServices;
using Vge.Item;

namespace Vge.Entity.Inventory
{
    /// <summary>
    /// Инвентарь массива предметов и одежды, в руке всегда первый предмет
    /// </summary>
    public class InventoryList : InventoryBase
    {
        /// <summary>
        /// Количество ячеек для предметов быстрого доступа
        /// </summary>
        protected readonly byte _mainCount;
        
        /// <summary>
        /// Количество ячеек для одежды
        /// </summary>
        protected readonly int _clothCount;

        /// <summary>
        /// Общее количество ячеек
        /// </summary>
        protected int _allCount;

        /// <summary>
        /// Выбранный слот правой руки
        /// </summary>
        protected byte _currentIndex = 0;

        /// <summary>
        /// Массив предметов инвентаря
        /// Вначале идут mainCount, потом clothCount
        /// </summary>
        protected readonly ItemStack[] _items;

        public InventoryList(byte mainCount, int clothCount)
        {
            _mainCount = mainCount;
            _clothCount = clothCount;
            _allCount = _mainCount + _clothCount;
            _items = new ItemStack[_allCount];
        }

        /// <summary>
        /// Получить выбранный слот правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte GetCurrentIndex() => _currentIndex;

        /// <summary>
        /// Задать активный слот быстрого доступа
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool SetCurrentIndex(byte slotIn)
        {
            if (slotIn < _mainCount && slotIn >= 0 && slotIn != _currentIndex)
            {
                _currentIndex = slotIn;
                _OnCurrentItemChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Сместить слот быстрого доступа в большую сторону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SlotMore()
        {
            if (_currentIndex < _mainCount - 1) _currentIndex++;
            else _currentIndex = 0;
            _OnCurrentItemChanged();
        }

        /// <summary>
        /// Сместить слот быстрого доступа в меньшую сторону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SlotLess()
        {
            if (_currentIndex > 0) _currentIndex--;
            else _currentIndex = (byte)(_mainCount - 1);
            _OnCurrentItemChanged();
        }

        /// <summary>
        /// Возвращает стaк в слоте slotIn
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ItemStack GetStackInSlot(int slotIn)
            => slotIn < _allCount ? _items[slotIn] : null;

        /// <summary>
        /// Устанавливает данный стек предметов в указанный слот в инвентаре
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetInventorySlotContents(int slotIn, ItemStack stack)
        {
            if (slotIn < _allCount)
            {
                _items[slotIn] = stack;
                _OnChanged(_currentIndex);
                if (slotIn == _currentIndex)
                {
                    _OnCurrentItemChanged();
                }
            }
        }

        /// <summary>
        /// Получить список стаков (что в руке и список одежды)
        /// для передачи по сети
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ItemStack[] GetCurrentItemAndCloth()
        {
            ItemStack[] stacks;
            if (_mainCount > 0)
            {
                stacks = new ItemStack[_clothCount + 1];
                stacks[0] = GetCurrentItem();
                Array.Copy(_items, _mainCount, stacks, 1, _clothCount);
            }
            else
            {
                stacks = new ItemStack[_clothCount];
                Array.Copy(_items, _mainCount, stacks, 0, _clothCount);
            }
            return stacks;
        }

        /// <summary>
        /// Задать список стаков (что в руке и список одежды)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetCurrentItemAndCloth(ItemStack[] stacks)
        {
            if (stacks.Length > 0)
            {
                if (_mainCount > 0) 
                {
                    // Имеется что в руке
                    SetCurrentItem(stacks[0]);
                    for (int i = 1; i < stacks.Length; i++)
                    {
                        SetInventorySlotContents(i - 1, stacks[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < stacks.Length; i++)
                    {
                        SetInventorySlotContents(i, stacks[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Получить стак одежды
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ItemStack GetClothInventory(int slotIn)
            => (_clothCount > 0 && slotIn < _clothCount) 
            ? _items[_mainCount + slotIn] : null;

        /// <summary>
        /// Задать стак одежды
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetClothInventory(int slotIn, ItemStack stack)
        {
            if (_clothCount > 0 && slotIn < _clothCount)
            {
                _items[_mainCount + slotIn] = stack;
            }
        }

        /// <summary>
        /// Получить выбранный стак правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ItemStack GetCurrentItem() => _mainCount > 0 ? _items[_currentIndex] : null;

        /// <summary>
        /// Задать в правую руку стак
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetCurrentItem(ItemStack stack)
        {
            if (_mainCount > 0)
            {
                //CheckKnowledge(stack);
                _items[_currentIndex] = stack;
                _OnChanged(_currentIndex);
                _OnCurrentItemChanged();
            }
        }

        /// <summary>
        /// Получить полный список всего инвентаря
        /// Mvk было GetMainAndCloth
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ItemStack[] GetAll() => _items;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _Clear()
        {
            for (int i = 0; i < _allCount; i++)
            {
                _items[i] = null;
            }
        }
    }
}
