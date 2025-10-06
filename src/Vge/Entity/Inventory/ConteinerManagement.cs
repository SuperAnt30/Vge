using Vge.Item;

namespace Vge.Entity.Inventory
{
    /// <summary>
    /// Управление контейнером, задача для переноса пачками, через Shift
    /// </summary>
    public class ConteinerManagement
    {
        /// <summary>
        /// Самый максимальный размер. Даже если значение ItemBase.MaxStackSize будет больше,
        /// обрежет по _inventoryStackLimit
        /// </summary>
        private const byte _inventoryStackLimit = 255;

        /// <summary>
        /// Количество изменений в рюкзаке
        /// </summary>
        private int _countBackpack;

        /// <summary>
        /// ID категории износа предмета, к примеру рюкзак = 1, карман = 2.
        /// По умолчанию = 0
        /// </summary>
        public byte IdDamageCategory;
        /// <summary>
        /// ID слота игнора износа предмета, к примеру выбранный предмет.
        /// По умолчанию = 255
        /// </summary>
        public byte IdDamageSlotIgnor = 255;

        /// <summary>
        /// Флаг пометки, что инвентарь креатива, имеет свои нюансы.
        /// </summary>
        public bool FlagCreativeMode;

        /// <summary>
        /// Свещение слота для отправки
        /// </summary>
        private readonly byte _biasSlot = 0;


        public ConteinerManagement(byte biasSlot = 0)=> _biasSlot = biasSlot;

        /// <summary>
        /// Добавляет стек предметов в инвентарь, возвращает false, если это невозможно. С проверкой на креатив
        /// </summary>
        /// <param name="stacks">Массив инвентаря или хранилища, с чем работаем</param>
        /// <param name="sourceIndex">С какого элемента работаем</param>
        /// <param name="length">Длинна массива стака, -1 возьмёт значение stacks.Length</param>
        public bool AddItemStackToInventory(ItemStack[] stacks, int sourceIndex, 
            ItemStack stack, int length)
        {
            if (stack != null && stack.Amount != 0 && stack.Item != null)
            {
                if (length == -1) length = stacks.Length;
                if (stack.IsItemDamaged())
                {
                    int slot = _GetFirstEmptyStack(stacks, sourceIndex, length);

                    if (slot >= 0)
                    {
                        stacks[slot] = stack.Copy();
                        stack.Zero();
                        _OnSendSetSlot(slot + _biasSlot, stacks[slot], stacks[slot].Amount);
                        if (IdDamageCategory != 0)
                        {
                            // Одно изменение в стаке рюкзака
                            _OnSendCountSlotsBackpack(IdDamageCategory, 1);
                        }
                        return true;
                    }
                    else if (FlagCreativeMode)
                    {
                        stack.Zero();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _countBackpack = 0;
                    int amount;
                    do
                    {
                        amount = stack.Amount;
                        stack.SetAmount(_StorePartialItemStack(stacks, stack, sourceIndex, length));
                    }
                    while (stack.Amount > 0 && stack.Amount < amount);

                    if (IdDamageCategory != 0)
                    {
                        if (_countBackpack > 0)
                        {
                            // Одно изменение в стаке рюкзака
                            _OnSendCountSlotsBackpack(IdDamageCategory, _countBackpack);
                        }
                    }

                    if (stack.Amount == amount && FlagCreativeMode)
                    {
                        stack.Zero();
                        return true;
                    }
                    else
                    {
                        return stack.Amount < amount;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Возвращает первый пустой стек элементов
        /// -1 нет пустого
        /// </summary>
        private int _GetFirstEmptyStack(ItemStack[] stacks, int sourceIndex, int length)
        {
            for (int i = sourceIndex; i < sourceIndex + length; i++)
            {
                if (stacks[i] == null) return i;
            }
            return -1;
        }

        /// <summary>
        /// Эта функция сохраняет как можно больше элементов ItemStack в соответствующем 
        /// слоте и возвращает количество оставшихся элементов.
        /// </summary>
        private byte _StorePartialItemStack(ItemStack[] stacks, ItemStack itemStack,
            int sourceIndex, int length)
        {
            ItemBase item = itemStack.Item;
            int amountBegin = itemStack.Amount;
            int amount = amountBegin;
            int slot = _StoreItemStack(stacks, itemStack, sourceIndex, length);

            if (slot < 0) slot = _GetFirstEmptyStack(stacks, sourceIndex, length);

            if (slot < 0)
            {
                return (byte)amount;
            }
            else
            {
                if (stacks[slot] == null)
                {
                    stacks[slot] = new ItemStack(item, 0, itemStack.ItemDamage);
                }

                int amount2 = amount;
                int amountCache = stacks[slot].Item.MaxStackSize - stacks[slot].Amount;
                if (amount > amountCache) amount2 = amountCache;
                amountCache = _inventoryStackLimit - stacks[slot].Amount;
                if (amount2 > amountCache) amount2 = amountCache;

                if (amount2 == 0)
                {
                    return (byte)amount;
                }
                else
                {
                    amount -= amount2;
                    stacks[slot].AddAmount((byte)amount2);
                    _OnSendSetSlot(slot + _biasSlot, stacks[slot], amount2);
                    if (IdDamageCategory == 0
                        || (IdDamageCategory != 0 && IdDamageSlotIgnor != slot))
                    {
                        _countBackpack++;
                    }
                    return (byte)amount;
                }
            }
        }

        /// <summary>
        /// Находим слот с таким же стакам, где можно ещё что-то засунуть. Типа не полный
        /// </summary>
        private int _StoreItemStack(ItemStack[] stacks, ItemStack itemStack, int sourceIndex, int length)
        {
            ItemStack stack;
            for (int i = sourceIndex; i < sourceIndex + length; i++)
            {
                stack = stacks[i];
                if (stack != null && stack.Item.IndexItem == itemStack.Item.IndexItem
                    && stack.IsStackable() && stack.Amount < stack.Item.MaxStackSize
                    && stack.Amount < _inventoryStackLimit
                    && stack.ItemDamage == itemStack.ItemDamage
                    && stack.IsItemEqual(itemStack))
                {
                    return i;
                }
            }
            return -1;
        }

        #region Event

        /// <summary>
        /// Отправить запрос изменённого слота 
        /// </summary>
        public event SlotEventHandler SendSetSlot;
        /// <summary>
        /// Отправить запрос изменённого слота 
        /// </summary>
        private void _OnSendSetSlot(int slot, ItemStack stack, int amount) 
            => SendSetSlot?.Invoke(this, new SlotEventArgs(slot, stack, amount));

        /// <summary>
        /// Отправить запрос количество изменённых слотов рюкзака
        /// </summary>
        public event SlotEventHandler SendCountSlotsBackpack;
        /// <summary>
        /// Отправить запрос изменённого слота 
        /// </summary>
        private void _OnSendCountSlotsBackpack(byte category, int count) 
            => SendCountSlotsBackpack?.Invoke(this, new SlotEventArgs(category, count));

        #endregion
    }
}
