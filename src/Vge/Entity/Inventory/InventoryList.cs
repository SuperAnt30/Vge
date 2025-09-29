using System;
using System.Runtime.CompilerServices;
using Vge.Item;
using Vge.Network.Packets.Server;
using Vge.World;

namespace Vge.Entity.Inventory
{
    /// <summary>
    /// Инвентарь массива предметов и одежды, в руке всегда первый предмет
    /// </summary>
    public class InventoryList : InventoryBase
    {
        /// <summary>
        /// Количество ячеек для предметов быстрого доступа (правая рука)
        /// </summary>
        protected readonly byte _mainCount;

        /// <summary>
        /// Количество ячеек для одежды.
        /// (Первый слот одеждый может быть ячейкой левой руки)
        /// </summary>
        protected readonly byte _clothCount;

        /// <summary>
        /// Общее количество ячеек
        /// </summary>
        protected byte _allCount;

        /// <summary>
        /// Выбранный слот правой руки
        /// </summary>
        protected byte _currentIndex = 0;

        /// <summary>
        /// Массив предметов инвентаря
        /// Вначале идут mainCount, потом clothCount
        /// </summary>
        protected readonly ItemStack[] _items;

        /// <summary>
        /// Побитовый флаг изменений, если =-1 значит все
        /// </summary>
        private int _flagsChanged;

        /// <summary>
        /// Массив ключей слотов одежды для ограничения установки предмета. 
        /// </summary>
        protected readonly byte[] _slotClothKeys;

        /// <param name="isLimitationSlot">Установить ограничение по ячейкам одежды, порядковая нумерация от 1-N,
        /// равенства ItemBase._slotClothIndex  </param>
        public InventoryList(byte mainCount, byte clothCount, bool isLimitationSlot = true) // 1 - 11 сетевой игрок
        {
            _mainCount = mainCount;
            _clothCount = clothCount;
            _InitAllCount();
            OutsideCount = _clothCount;
            if (_mainCount > 0) OutsideCount++;

            _items = new ItemStack[_allCount];
            _slotClothKeys = new byte[_clothCount];
            if (isLimitationSlot && _clothCount > 0)
            {
                for (byte b = 1; b <= _clothCount; b++)
                {
                    _slotClothKeys[b - 1] = b;
                }
            }
        }

        /// <summary>
        /// Инициализация общего количества
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _InitAllCount()
            => _allCount = (byte)(_mainCount + _clothCount);

        #region UpdateServer

        /// <summary>
        /// Обновление на сервере каждый тик
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateServer(EntityBase entity, WorldServer worldServer)
        {
            if (_flagsChanged != 0)
            {
                for (byte i = 0; i < OutsideCount; i++)
                {
                    if (_flagsChanged == -1 || (_flagsChanged & (1 << i)) != 0)
                    {
                        // Слот менялся
                        worldServer.Tracker.SendToAllTrackingEntity(entity, 
                            new PacketS10EntityEquipment(entity.Id, i,
                            _mainCount > 0
                                ? _items[i == 0 ? _currentIndex : _mainCount + i - 1] 
                                : _items[i]));
                    }
                }
                _flagsChanged = 0;
            }
        }

        #endregion

        /// <summary>
        /// Получить выбранный слот правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte GetCurrentIndex() => _currentIndex;

        /// <summary>
        /// Задать активный слот правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool SetCurrentIndex(byte slotIn)
        {
            if (slotIn < _mainCount && slotIn >= 0 && slotIn != _currentIndex)
            {
                _currentIndex = slotIn;
                _OnCurrentIndexChanged();
                _OnOutsideChanged(1); // 0 - правая рука
                return true;
            }
            return false;
        }

        /// <summary>
        /// Сместить слот правой руки в большую сторону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SlotMore()
        {
            if (_currentIndex < _mainCount - 1) _currentIndex++;
            else _currentIndex = 0;
            _OnCurrentIndexChanged();
        }

        /// <summary>
        /// Сместить слот правой руки в меньшую сторону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SlotLess()
        {
            if (_currentIndex > 0) _currentIndex--;
            else _currentIndex = (byte)(_mainCount - 1);
            _OnCurrentIndexChanged();
        }

        /// <summary>
        /// Получить текущий стак правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ItemStack GetCurrentItem() => _mainCount > 0 ? _items[_currentIndex] : null;

        /// <summary>
        /// Задать в текущий стак правой руки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetCurrentItem(ItemStack stack)
        {
            if (_mainCount > 0)
            {
                //CheckKnowledge(stack);
                _items[_currentIndex] = stack;
                _OnSlotChanged(_currentIndex);
                _OnOutsideChanged(1); // 0 - правая рука
            }
        }

        /// <summary>
        /// Возвращает стaк в слоте slotIn
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ItemStack GetStackInSlot(int slotIn)
            => slotIn < _allCount ? _items[slotIn] : null;

        /// <summary>
        /// Устанавливает стaк в слоте slotIn, без событий!!!, для загрузки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetStackInSlot(int slotIn, ItemStack stack)
        {
            if (slotIn < _allCount) _items[slotIn] = stack;
        }

        /// <summary>
        /// Устанавливает данный стак предметов в указанный слот в инвентаре
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetInventorySlotContents(int slotIn, ItemStack stack)
        {
            if (slotIn < _allCount)
            {
                _items[slotIn] = stack;
                // События для визуализации внешности
                if (_mainCount > 0)
                {
                    // Имеется предмет в правой руке 0, 1-N одежда
                    if (slotIn == _currentIndex)
                    {
                        _OnOutsideChanged(1); // 0 - правая рука
                    }
                    else if (slotIn >= _mainCount
                        && slotIn <= _clothCount) // Равно это на один больше, так-как один предмет в левой руке
                    {
                        // одежда с минусом правой руки
                        _OnOutsideChanged(1 << (slotIn - _mainCount + 1));
                    }
                }
                else
                {
                    // Только одежда, и возможно левая рука 0-N
                    _OnOutsideChanged(1 << slotIn);
                }
            }
        }

        /// <summary>
        /// Получить стак по слоту внешности (что в руках и одежда)
        /// для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ItemStack GetOutside(int slotIn)
            => _items[_mainCount > 0 ? slotIn == 0 ? _currentIndex : _mainCount + slotIn - 1 : slotIn];

        /// <summary>
        /// Получить список стаков для внешности (что в руках и одежда)
        /// для передачи по сети
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ItemStack[] GetOutside()
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
        /// Задать список стаков для внешности (что в руках и одежда)
        /// У данного персонажа должно быть _mainCount=0 || _mainCount=1 и _allCount == stacks.Length
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetOutside(ItemStack[] stacks)
        {
            if (stacks.Length > 0 && _allCount == stacks.Length)
            {
                for (int i = 0; i < stacks.Length; i++)
                {
                    _items[i] = stacks[i];
                }
                _OnOutsideChanged(-1); // любой
            }
        }

        /// <summary>
        /// Получить ключи слота одежды, для ограничения установки предмета. Если слот не одежды вернёт 0
        /// Если =0, то можно любой устанавливать.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte GetSlotClothKey(int slotIn) 
            => (_clothCount > 0 && slotIn >= _mainCount && slotIn < _mainCount + _clothCount)
            ? _slotClothKeys[slotIn - _mainCount] : (byte)0;

        /// <summary>
        /// Получить ключи слота внешности, для ограничения установки предмета.
        /// для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte GetOutsideSlotKey(int slotIn)
            => _mainCount > 0 && slotIn != 0 ? _slotClothKeys[slotIn - 1] : (byte)0;

        /*
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
                _OnSlotChanged(slotIn);
                if (_mainCount > 0)
                {
                    _OnOutsideChanged(1 << (slotIn - _mainCount + 1)); // одежда с минусом правой руки
                }
                else
                {
                    _OnOutsideChanged(1 << slotIn); // одежда
                }
            }
        }
        */

        /// <summary>
        /// Получить полный список всего инвентаря
        /// Mvk было GetMainAndCloth
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ItemStack[] GetAll() => _items;

        /// <summary>
        /// Задать полный список всего инвентаря
        /// Mvk было SetMainAndCloth
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetAll(ItemStack[] stacks)
        {
            if (stacks.Length == _items.Length)
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    _items[i] = stacks[i];
                }
                _OnOutsideChanged(-1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _Clear()
        {
            for (int i = 0; i < _allCount; i++)
            {
                _items[i] = null;
            }
        }

        protected override void _OnOutsideChanged(int flags)
        {
            if (_flagsChanged != -1)
            {
                if (flags == -1)
                {
                    _flagsChanged = -1;
                }
                else
                {
                    for (int i = 0; i < OutsideCount; i++)
                    {
                        if ((flags & (1 << i)) != 0
                            && (_flagsChanged & (1 << i)) == 0)
                        {
                            _flagsChanged += 1 << i;
                        }
                    }
                }
            }
            base._OnOutsideChanged(flags);
        }
    }
}
