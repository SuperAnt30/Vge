using Mvk2.Entity.List;
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
        /// Лимит ячеек рюкзака
        /// </summary>
        public byte LimitBackpack { get; private set; } = 0;
        /// <summary>
        /// Количество ячеек для предметов рюкзака
        /// </summary>
        private readonly byte _backpackCount = 25;
        /// <summary>
        /// Игрок сервера
        /// </summary>
        private readonly PlayerServerMvk _playerServer;
        /// <summary>
        /// Управление контейнером для передачи пачками
        /// </summary>
        private readonly ConteinerManagement _conteiner;
        /// <summary>
        /// Открыто ли окно инвентаря, для сервера
        /// </summary>
        private bool _isOpenInventory;

        public InventoryPlayer(PlayerServerMvk playerServer)
            // Первый слот одеждый это ячейка левой руки
            : base(8, 11)
        {
            _playerServer = playerServer;
            if (playerServer != null)
            {
                _conteiner = new ConteinerManagement();
                // Перенос стака шифтом только на сервере фиксация
                _conteiner.SendSetSlot += (sender, e) => _OnSlotSetted(e);
                _conteiner.SendCountSlotsBackpack += (sender, e) 
                    => _DamageCaregory(e.SlotId, e.Amount);
            }
        }

        /// <summary>
        /// Инициализация общего количества
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitAllCount()
            => _allCount = (byte)(_mainCount + _clothCount + _backpackCount);

        #region Server

        /// <summary>
        /// Открыли инвентарь, для сервера
        /// </summary>
        public void ServerOpenInventory() => _isOpenInventory = true;

        /// <summary>
        /// Закрыли инвентарь, для сервера
        /// </summary>
        public void ServerCloseInventory() => _isOpenInventory = false;

        /// <summary>
        /// Клик по указанному слоту в инвентаре на сервере!
        /// isRight указывает флаг правый ли клик мыши
        /// 0-99 слот персонажа, а именно (slot 0-7 инвентарь, 8-11 броня),
        /// 100-254 слот у блока с TileEntity по позиции
        /// 255 слот предмет в воздухе StackAir, быть не должен!
        /// </summary>
        public void ClickInventoryOnServer(int slotIn, bool isRight, bool isShift)
        {
            ItemStack stackAir = StackAir?.Copy();
            ItemStack stackSlot = _GetStackInSlotAndStorage(slotIn)?.Copy();

            if (stackSlot == null) 
            {
                // Клик по пустому слоту
                if (stackAir != null)
                {
                    // В воздухе имеется, укладываем в пустой слот
                    if (_CanPutItemStack(slotIn, stackAir))
                    {
                        if (slotIn >= 100 // Проверка в ячейку хранилища
                            || slotIn < _allCount - _backpackCount + LimitBackpack) // Проверка по размеру рюкзака
                        {
                            // Если в воздухе есть так будем укладывать в ячейку
                            if (isRight && stackAir.Amount > 1)
                            {
                                // Если был клик правой клавишей, то укладываем только одну единицу
                                _SetSendSlotContents(slotIn, stackAir.Copy().SetAmount(1));
                                _SetSendAirContents(stackAir.ReduceAmount(1));
                            }
                            else
                            {
                                // Перекладываем всё из воздуха в ячейку
                                _SetSendAirContents();
                                _SetSendSlotContents(slotIn, stackAir);
                            }
                        }
                    }
                }
            }
            else
            {
                // Имеется что-то в ячейке
                if (isShift)
                {
                    // Если держим шифт, то задача перебрасывать предмет с инвентаря
                    // в свободную ячейку склада или наоборот
                    if (slotIn < 100)
                    {
                        // Кликнули на инвентарь
                        if (_isOpenInventory)
                        {
                            // Тут клики через шифт по инвентарю
                            if (slotIn < _mainCount)
                            {
                                // Кликнули на инвентарь быстрого доступа
                                _conteiner.IdDamageCategory = 1;
                                if (!_conteiner.AddItemStackToInventory(_items, _mainCount + _clothCount, 
                                    _CheckSlotToAir(stackSlot), LimitBackpack))
                                {
                                    _SetSendSlotContents(slotIn, stackSlot);
                                }
                                else
                                {
                                    _SetSendSlotContents(slotIn);
                                }
                            }
                            else
                            {
                                // Кликнули на рюкзак
                                _conteiner.IdDamageCategory = 2;
                                _conteiner.IdDamageSlotIgnor = _currentIndex;
                                if (!_conteiner.AddItemStackToInventory(_items, 0, stackSlot, _mainCount))
                                {
                                    _SetSendSlotContents(slotIn, stackSlot);
                                }
                                else
                                {
                                    _SetSendSlotContents(slotIn);
                                }
                                _conteiner.IdDamageSlotIgnor = 255;
                            }
                            _conteiner.IdDamageCategory = 0;
                        }
                        else
                        {
                            return;
                            //TileEntityBase tileEntity = ((EntityPlayerServer)Player).GetTileEntityAction();
                            //if (tileEntity != null)
                            //{
                            //    if (!tileEntity.AddItemStackToInventory(CheckSlotToAir(stackSlot)))
                            //    {
                            //        _SetSendSlotContents(slotIn, stackSlot);
                            //    }
                            //    else
                            //    {
                            //        _SetSendSlotContents(slotIn);
                            //    }
                            //}
                        }
                    }
                    else
                    {
                        if (_CanPutItemStack(slotIn, stackSlot))
                        {
                            // Кликнули в хранилище
                            _conteiner.IdDamageCategory = 2;
                            _conteiner.IdDamageSlotIgnor = _currentIndex;
                            if (!_conteiner.AddItemStackToInventory(_items, 0, stackSlot, _mainCount))
                            {
                                _SetSendSlotContents(slotIn, stackSlot);
                            }
                            else
                            {
                                _SetSendSlotContents(slotIn);
                            }
                            _conteiner.IdDamageCategory = 0;
                            _conteiner.IdDamageSlotIgnor = 255;
                        }
                    }
                }
                else
                {
                    if (stackAir == null)
                    {
                        // В воздухе нет ничего
                        if (isRight && stackSlot.Amount > 1)
                        {
                            // Берём половину в воздух
                            byte amount = (byte)(stackSlot.Amount / 2);
                            _SetSendAirContents(_CheckSlotToAir(stackSlot.Copy().SetAmount(amount)));
                            _SetSendSlotContents(slotIn, stackSlot.ReduceAmount(amount));
                        }
                        else
                        {
                            // Перекладываем всё из ячейку в воздух
                            _SetSendAirContents(_CheckSlotToAir(stackSlot));
                            _SetSendSlotContents(slotIn);
                        }
                    }
                    else
                    {
                        if (_CanPutItemStack(slotIn, stackAir))
                        {
                            // В воздухе имеется и в ячейке имеется
                            if (stackSlot.Item.IndexItem == stackAir.Item.IndexItem && stackSlot.ItemDamage == stackAir.ItemDamage)
                            {
                                // Если предметы одинаковые
                                if (isRight && stackAir.Amount > 1)
                                {
                                    if (stackSlot.Item.MaxStackSize > stackSlot.Amount)
                                    {
                                        // Если был клик правой клавишей, то добавляем только одну единицу
                                        _SetSendSlotContents(slotIn, stackSlot.AddAmount(1));
                                        _SetSendAirContents(stackAir.ReduceAmount(1));
                                    }
                                }
                                else
                                {
                                    byte aw = (byte)(stackSlot.Amount + stackAir.Amount);
                                    if (aw > stackSlot.Item.MaxStackSize)
                                    {
                                        // сумма больше слота значит не весь перекладываем в руке останется
                                        _SetSendSlotContents(slotIn, stackSlot.SetAmount(stackSlot.Item.MaxStackSize));
                                        _SetSendAirContents(stackAir.SetAmount((byte)(aw - stackSlot.Item.MaxStackSize)));
                                    }
                                    else
                                    {
                                        // весь можно переложить
                                        _SetSendSlotContents(slotIn, stackSlot.SetAmount(aw));
                                        _SetSendAirContents();
                                    }
                                }
                            }
                            else
                            {
                                // Если разные, меняем местами
                                _SetSendAirContents(_CheckSlotToAir(stackSlot));
                                _SetSendSlotContents(slotIn, stackAir);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Выкинуть предмет из рук который в воздухе.
        /// Server
        /// </summary>
        public void ThrowOutAir()
        {
            if (StackAir != null)
            {
                _playerServer?.DropItem(StackAir, true, false);
                _SetSendAirContents();
            }
        }

        /// <summary>
        /// Дропнуть предметы с рюкзака начиная с from
        /// </summary>
        private void _DropItemBackpack(int from)
        {
            ItemStack stack;
            for (int i = from; i < _allCount; i++)
            {
                stack = _items[i];
                if (stack != null)
                {
                    _playerServer?.DropItem(stack, true, false);
                    _SetSendSlotContents(i);
                }
            }
        }

        /// <summary>
        /// Задать потом отправить игроку изменения слота со стакам.
        /// Server
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SetSendSlotContents(int slotIn, ItemStack stack = null)
        {
            SetInventorySlotContents(slotIn, stack);
            if (slotIn != 255 && slotIn > 99)
            {
                // Изменения ячейки хранилища в TileEntity
                _OnSlotStorageChanged(slotIn);
            }
        }

        /// <summary>
        /// Задать потом отправить игроку изменения воздушного стака.
        /// Server
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SetSendAirContents(ItemStack stack = null)
            => SetInventorySlotContents(255, stack);

        /// <summary>
        /// Проверка и замена стака если мы берём в воздух.
        /// Server
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ItemStack _CheckSlotToAir(ItemStack stack) => stack.CheckToAir();

        #endregion

        /// <summary>
        /// Проверяем можно ли установить данный стак в определённой ячейке склада
        /// </summary>
        private bool _CanPutItemStack(int slotIn, ItemStack stack)
        {
            //if (tileEntityCache == null)
            //{
            if (slotIn < 100)// && slotIn >= _allCount)
            {
                // Для одежды свои правила
                byte key = GetSlotClothKey(slotIn);
                return key == 0 || (stack != null && stack.Item.CheckSlotClothKey(key));
            }
            return true;
            //}

            //return tileEntityCache.CanPutItemStack(stack);
        }

        /// <summary>
        /// Возвращает стак слота, из инвентаря или хранилища выбранного блока
        /// </summary>
        private ItemStack _GetStackInSlotAndStorage(int slotIn)
        {
            //tileEntityCache = null;
            if (slotIn > 99)
            {
                // слот склада
                //if (Player is EntityPlayerServer entityPlayerServer)
                //{
                //    tileEntityCache = entityPlayerServer.GetTileEntityAction();
                //    if (tileEntityCache != null)
                //    {
                //        return tileEntityCache.GetStackInSlot(slotIn - 100);
                //    }
                //}
                return null;
            }
            // слот у игрока
            return GetStackInSlot(slotIn);
        }

        /// <summary>
        /// Устанавливает данный стак предметов в указанный слот в инвентаре
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetInventorySlotContents(int slotIn, ItemStack stack)
        {
            if (slotIn == 255)
            {
                // Воздух
                StackAir = stack;
                _OnSlotSetted(slotIn, stack);
            }
            else if(slotIn < _allCount)
            {
                _items[slotIn] = stack;
                _OnSlotSetted(slotIn, stack);
                // События для визуализации внешности, 0 правая рука 1 левая рука 2-11 одежда
                if (slotIn == _currentIndex)
                {
                    _OnOutsideChanged(1); // 0 - правая рука
                }
                else if (slotIn >= _mainCount && slotIn - _mainCount < _clothCount)
                {
                    // Тут при смене одежды
                    _OnOutsideChanged(1 << (slotIn - _mainCount + 1)); // 1-11 одежда
                    if (slotIn != _mainCount) // если равно, это Левая рука
                    {
                        CheckingClothes(_playerServer != null);
                       // Console.WriteLine((_playerServer == null ? "C " : "S ") + slotIn + " " + (stack == null ? "" : stack.ToString()));
                    }
                }
                else if (slotIn >= _mainCount + _clothCount)
                {
                    // Рюкзак
                    if (stack != null && _playerServer != null)
                    {
                        _DamageCaregory(1, 1);
                    }
                }
                else if (slotIn < _mainCount)
                {
                    // Быстрый доступ (правая рука) 
                    // !!! Сюда не поподаем в выбранную руку !!!, так задумано
                    if (stack != null && _playerServer != null)
                    {
                        _DamageCaregory(2, 1);
                    }
                }
            }
            //else // TODO:: 2025-09-22 добавить склад, рюкзак.
            //{
            //   // base.SetInventorySlotContents(slotIn, stack);
            //    _OnSlotSetted(slotIn, stack);
            //}

            
        }

        /// <summary>
        /// Проверка одетого
        /// </summary>
        public void CheckingClothes(bool isServer)
        {
            // Проверка лимита рюкзака, и прочей брони
            int count = _mainCount + _clothCount;
            ItemStack stack;
            int limitBackpack = 0;
            for (int i = _mainCount; i < count; i++)
            {
                stack = _items[i];
                if (stack != null && stack.Item is ItemCloth itemCloth && itemCloth.CellsBackpack != 0)
                {
                    limitBackpack += itemCloth.CellsBackpack;
                }
            }
            if (LimitBackpack != limitBackpack)
            {
                if (isServer && LimitBackpack > limitBackpack)
                {
                    // Надо выкинуть часть предметов только на сервере
                    _DropItemBackpack(count + limitBackpack);
                }
                if (limitBackpack > _backpackCount)
                {
                    limitBackpack = _backpackCount;
                }
                LimitBackpack = (byte)limitBackpack;
                _OnLimitBackpackChanged();
            }
        }

        /// <summary>
        /// Задать полный список всего инвентаря
        /// Mvk было SetMainAndCloth
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetAll(ItemStack[] stacks)
        {
            base.SetAll(stacks);
            // Тут поподаем при загрузке на клиент
            CheckingClothes(false);
        }

        /// <summary>
        /// Надламываем предмет одежды, к примеру рюкзак если прилетел предмет stack в слот рюкзака
        /// </summary>
        private void _DamageCaregory(int category, int amount)
        {
           // Console.WriteLine("Damage [" + category + "] " + amount);
            //if (clothInventory[ID_SLOT_BACKPACK] != null)
            //{
            //    // Урон рюкзаку
            //    clothInventory[ID_SLOT_BACKPACK].DamageItemCloth(Player.World, amount, ID_SLOT_BACKPACK, Player);
            //    OnChanged(IdSlotBackpackAll);
            //}
        }

        #region Event

        /// <summary>
        /// Событие изменён лимит рюкзака
        /// </summary>
        public event EventHandler LimitBackpackChanged;
        /// <summary>
        /// Событие изменён лимит рюкзака
        /// </summary>
        private void _OnLimitBackpackChanged()
            => LimitBackpackChanged?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие слот задан
        /// </summary>
        public event SlotEventHandler SlotSetted;
        /// <summary>
        /// Событие слот задан
        /// </summary>
        private void _OnSlotSetted(int slotId, ItemStack stack)
            => SlotSetted?.Invoke(this, new SlotEventArgs(slotId, stack));
        /// <summary>
        /// Событие слот задан
        /// </summary>
        private void _OnSlotSetted(SlotEventArgs e)
            => SlotSetted?.Invoke(this, e);

        /// <summary>
        /// Событие слот хранилища (TileEntity) изменён
        /// </summary>
        public event SlotEventHandler SlotStorageChanged;
        /// <summary>
        /// Событие слот хранилища (TileEntity) изменён
        /// </summary>
        private void _OnSlotStorageChanged(int slotId)
            => SlotStorageChanged?.Invoke(this, new SlotEventArgs(slotId));


        // TODO:: 2025-10-01 удалить
        /// <summary>
        /// Событие изменён слот
        /// </summary>
        public event SlotEventHandler SlotChanged;
        /// <summary>
        /// Событие изменён слот
        /// </summary>
        protected override void _OnSlotChanged(int slotId)
            => SlotChanged?.Invoke(this, new SlotEventArgs(slotId));

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
