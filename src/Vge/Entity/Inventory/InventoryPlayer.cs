using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Item;
using Vge.World.BlockEntity;

namespace Vge.Entity.Inventory
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
        /// Лимит ячеек кармана
        /// </summary>
        public byte LimitPocket { get; private set; }
        /// <summary>
        /// Лимит ячеек рюкзака
        /// </summary>
        public byte LimitBackpack { get; private set; } = 0;

        /// <summary>
        /// Обязательные карманы, образно рука и за пояс
        /// </summary>
        private readonly byte _requiredPocket = 2;

        /// <summary>
        /// Количество ячеек для предметов рюкзака
        /// </summary>
        private readonly byte _backpackCount;
        /// <summary>
        /// Игрок сервера
        /// </summary>
        private readonly PlayerServer _playerServer;
        /// <summary>
        /// Управление контейнером для передачи пачками
        /// </summary>
        private readonly ConteinerManagement _conteiner;

        /// <summary>
        /// Активный блок инвентаря (склад), если null, то работаем с инвентарём игрока.
        /// Только для сервера
        /// </summary>
        private IBlockStorage _blockStorage;

        public InventoryPlayer(PlayerServer playerServer, byte pocketCount, byte clothCount, byte backpackCount)
            // Первый слот одеждый это ячейка левой руки
            : base(pocketCount, clothCount, (byte)(pocketCount + clothCount + backpackCount))
        {
            _backpackCount = backpackCount;
            _playerServer = playerServer;
            LimitPocket = _requiredPocket;
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
        /// Сместить слот правой руки в большую сторону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SlotMore()
        {
            if (_currentIndex < LimitPocket - 1) _currentIndex++;
            else _currentIndex = 0;
            _OnCurrentIndexChanged();
            _OnOutsideChanged(1); // 0 - правая рука
        }

        /// <summary>
        /// Сместить слот правой руки в меньшую сторону
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SlotLess()
        {
            if (_currentIndex > 0) _currentIndex--;
            else _currentIndex = (byte)(LimitPocket - 1);
            _OnCurrentIndexChanged();
            _OnOutsideChanged(1); // 0 - правая рука
        }

        #region Server

        /// <summary>
        /// Добавляет стек предметов в инвентарь, возвращает false, если это невозможно.
        /// </summary>
        public override bool AddItemStackToInventory(ItemStack itemStack)
        {
            ItemStack stackSlot = itemStack?.Copy();

            // Сразу в карманы пробуем
            _conteiner.IdDamageCategory = 2;
            _conteiner.IdDamageSlotIgnor = _currentIndex;
            if (!_conteiner.AddItemStackToInventory(_items, 0, stackSlot, LimitPocket)) // * в карман
            {
                // Если не залезло всё, суём в рюкзак
                _conteiner.IdDamageCategory = 1;
                _conteiner.IdDamageSlotIgnor = 255;
                if (!_conteiner.AddItemStackToInventory(_items, _pocketCount + _clothCount, // * в рюкзак
                    stackSlot, LimitBackpack))
                {
                    // Что-то осталось
                    itemStack.SetAmount(stackSlot.Amount);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Получить активный блок хранилища, если нет вернёт null
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IBlockStorage GetBlockStorage() => _blockStorage;

        /// <summary>
        /// Открыли инвентарь, для сервера
        /// </summary>
        public void ServerOpenInventory(IBlockStorage blockStorage)
        {
            _blockStorage = blockStorage;
            _blockStorage.OpenWindow(_playerServer);
        }

        /// <summary>
        /// Закрыли инвентарь, для сервера
        /// </summary>
        public void ServerCloseInventory() => _blockStorage = null;

        /// <summary>
        /// Проверка по лимитам
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _CheckSlotLimit(int slotIn)
        {
            // Проверка склада
            if (slotIn > 99) return true;
            // Проверка по размеру кармана
            if (slotIn < _pocketCount) return LimitPocket > slotIn;
            // Проверка по размеру рюкзака
            return slotIn < _allCount - _backpackCount + LimitBackpack;
        }

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
                        if (_CheckSlotLimit(slotIn)) // Проверка по размеру рюкзака
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
                        if (_blockStorage == null)
                        {
                            // Тут клики через шифт по инвентарю
                            if (slotIn < _pocketCount)
                            {
                                // Кликнули на инвентарь быстрого доступа
                                _conteiner.IdDamageCategory = 1;
                                if (!_conteiner.AddItemStackToInventory(_items, _pocketCount + _clothCount, 
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
                                if (!_conteiner.AddItemStackToInventory(_items, 0, stackSlot, LimitPocket))
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
                            if (!_blockStorage.AddItemStackToInventory(_CheckSlotToAir(stackSlot)))
                            {
                                _SetSendSlotContents(slotIn, stackSlot);
                            }
                            else
                            {
                                _SetSendSlotContents(slotIn);
                            }
                        }
                    }
                    else
                    {
                        if (_CanPutItemStack(slotIn, stackSlot))
                        {
                            if (_blockStorage == null)
                            {
                                // Кликнули из рюкзака
                                _conteiner.IdDamageCategory = 2;
                                _conteiner.IdDamageSlotIgnor = _currentIndex;
                                if (!_conteiner.AddItemStackToInventory(_items, 0, stackSlot, _pocketCount))
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
                            else
                            {
                                // Кликнули из хранилища
                                _conteiner.IdDamageCategory = 2;
                                _conteiner.IdDamageSlotIgnor = _currentIndex;
                                if (!_conteiner.AddItemStackToInventory(_items, 0, stackSlot, LimitPocket)) // в карман
                                {
                                    _conteiner.IdDamageCategory = 1;
                                    _conteiner.IdDamageSlotIgnor = 255;
                                    if (!_conteiner.AddItemStackToInventory(_items, _pocketCount + _clothCount, // в рюкзак
                                        stackSlot, LimitBackpack))
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
                                    _SetSendSlotContents(slotIn);
                                }
                                _conteiner.IdDamageCategory = 0;
                                _conteiner.IdDamageSlotIgnor = 255;
                            }
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
        /// Дропнуть предметы с from по count - 1
        /// </summary>
        private void _DropItems(int from, int count)
        {
            ItemStack stack;
            for (int i = from; i < count; i++)
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
                _OnSlotStorageChanged(slotIn, stack);
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

        /// <summary>
        /// Проверяем можно ли установить данный стак в определённой ячейке склада
        /// </summary>
        private bool _CanPutItemStack(int slotIn, ItemStack stack)
        {
            if (slotIn < 100)
            {
                // Для одежды свои правила
                byte key = GetSlotClothKey(slotIn);
                return key == 0 || (stack != null && stack.Item.CheckSlotClothKey(key));
            }
            else if (_blockStorage != null)
            {
                return _blockStorage.CanPutItemStack(slotIn - 100, stack);
            }
            return false;
        }

        /// <summary>
        /// Возвращает стак слота, из инвентаря или хранилища выбранного блока
        /// </summary>
        private ItemStack _GetStackInSlotAndStorage(int slotIn)
        {
            if (slotIn > 99)
            {
                // слот склада
                if (_blockStorage != null)
                {
                    return _blockStorage.GetStackInSlot(slotIn - 100);
                }
                return null;
            }
            // слот у игрока
            return GetStackInSlot(slotIn);
        }

        #endregion

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
            else if (slotIn < _allCount)
            {
                _items[slotIn] = stack;
                _OnSlotSetted(slotIn, stack);
                // События для визуализации внешности, 0 правая рука 1 левая рука 2-11 одежда
                if (slotIn == _currentIndex)
                {
                    _OnOutsideChanged(1); // 0 - правая рука
                }
                else if (slotIn >= _pocketCount && slotIn - _pocketCount < _clothCount)
                {
                    // Тут при смене одежды
                    int flag = 1 << (slotIn - _pocketCount + 1);
                    if (slotIn != _pocketCount) // если равно _pocketCount, это Левая рука
                    {
                        if (CheckingClothes(_playerServer != null))
                        {
                            // Тут если изменился лимит кармана и выбранный предмет правой руки изменился
                            flag += 1; // 0 - правая рука
                        }
                    }
                    _OnOutsideChanged(flag); // 1-11 одежда
                }
                else if (slotIn >= _pocketCount + _clothCount)
                {
                    // Рюкзак
                    if (stack != null && _playerServer != null)
                    {
                        _DamageCaregory(1, 1);
                    }
                }
                else if (slotIn < _pocketCount)
                {
                    // Быстрый доступ (правая рука) 
                    // !!! Сюда не поподаем в выбранную руку !!!, так задумано
                    if (stack != null && _playerServer != null)
                    {
                        _DamageCaregory(2, 1);
                    }
                }
            }
            else if (_blockStorage != null) // только сервер
            {
                // Склад
                _blockStorage.SetStackInSlot(slotIn - 100, stack);
                //_OnSlotSetted(slotIn, stack); Это не надо, так-как склад рассылается всем кто его видит в другом месте
            }
        }

        /// <summary>
        /// Проверка одетого, вернёт true если изменился лимит кармана и смещён выбранный предмет правой руки
        /// </summary>
        public bool CheckingClothes(bool isServer)
        {
            // Проверка лимита рюкзака, и прочей брони
            int count = _pocketCount + _clothCount;
            ItemStack stack;
            int limitPocket = _requiredPocket;
            int limitBackpack = 0;
            bool right = false;
            for (int i = _pocketCount; i < count; i++)
            {
                stack = _items[i];
                if (stack != null && stack.Item is ItemCloth itemCloth)
                {
                    if (itemCloth.CellsPocket != 0)
                    {
                        limitPocket += itemCloth.CellsPocket;
                    }
                    if (itemCloth.CellsBackpack != 0) 
                    {
                        limitBackpack += itemCloth.CellsBackpack;
                    }
                    
                }
            }
            if (limitPocket > _pocketCount) limitPocket = _pocketCount;

            if (LimitPocket != limitPocket)
            {
                if (isServer && LimitPocket > limitPocket)
                {
                    // Надо выкинуть часть предметов только на сервере
                    _DropItems(limitPocket, _pocketCount);
                }
                LimitPocket = (byte)limitPocket;
                _OnLimitPocketChanged();
                if (!isServer) // Только на клиенте
                {
                    if (_currentIndex >= LimitPocket)
                    {
                        _currentIndex = (byte)(LimitPocket - 1);
                        _OnCurrentIndexChanged();
                        right = true;
                    }
                }
            }

            if (limitBackpack > _backpackCount) limitBackpack = _backpackCount;

            if (LimitBackpack != limitBackpack)
            {
                if (isServer && LimitBackpack > limitBackpack)
                {
                    // Надо выкинуть часть предметов только на сервере
                    _DropItems(count + limitBackpack, _allCount);
                }
               
                LimitBackpack = (byte)limitBackpack;
                _OnLimitBackpackChanged();
            }

            return right;
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
            Console.WriteLine("Damage [" + category + "] " + amount);
            //if (clothInventory[ID_SLOT_BACKPACK] != null)
            //{
            //    // Урон рюкзаку
            //    clothInventory[ID_SLOT_BACKPACK].DamageItemCloth(Player.World, amount, ID_SLOT_BACKPACK, Player);
            //    OnChanged(IdSlotBackpackAll);
            //}
        }

        #region Event

        /// <summary>
        /// Событие изменён лимит карманов
        /// </summary>
        public event EventHandler LimitPocketChanged;
        /// <summary>
        /// Событие изменён лимит карманов
        /// </summary>
        private void _OnLimitPocketChanged()
            => LimitPocketChanged?.Invoke(this, new EventArgs());

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
        private void _OnSlotStorageChanged(int slotId, ItemStack stack)
            => SlotStorageChanged?.Invoke(this, new SlotEventArgs(slotId, stack));


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
