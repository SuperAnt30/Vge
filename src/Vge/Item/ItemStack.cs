using System.Runtime.CompilerServices;
using Vge.NBT;
using Vge.Network;

namespace Vge.Item
{
    /// <summary>
    /// Объект одной ячейки предметов
    /// </summary>
    public class ItemStack
    {
        /// <summary>
        /// Объект предмета
        /// </summary>
        public ItemBase Item { get; private set; }
        /// <summary>
        /// Урон или износ
        /// </summary>
        public int ItemDamage { get; private set; }
        /// <summary>
        /// Количество вещей в стаке
        /// </summary>
        public byte Amount { get; private set; }

        public ItemStack(ItemBase item, byte amount, int itemDamage)
        {
            Item = item;
            Amount = amount;
            ItemDamage = itemDamage;
        }
        public ItemStack(ItemBase item, byte amount)
        {
            Item = item;
            Amount = amount;
        }
        public ItemStack(ItemBase item)
        {
            Item = item;
            Amount = 1;
        }

        /// <summary>
        /// Копия стака
        /// </summary>
        public ItemStack Copy() => new ItemStack(Item, Amount, ItemDamage);

        /// <summary>
        /// Добавить к количеству
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemStack AddAmount(byte amount)
        {
            Amount += amount;
            return this;
        }
        /// <summary>
        /// Уменьшить количество на amount
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemStack ReduceAmount(byte amount)
        {
            Amount -= amount;
            if (Amount < 0) Amount = 0;
            return this;
        }
        /// <summary>
        /// Задать новое количество
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemStack SetAmount(byte amount)
        {
            Amount = amount;
            return this;
        }

        /// <summary>
        /// Количество сделать нуль
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero() => Amount = 0;

        /// <summary>
        /// Проверка и замена стака если мы берём в воздух блокировка для положить в инвентарь и или ящик
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemStack CheckToAir()
        {
            if (Item != null && Item.CheckToAir())
            {
                Item = Ce.Items.ItemObjects[Item.IndexReplaceItemDueAir()];
            }
            return this;
        }

        #region Is

        /// <summary>
        /// Возвращает true, когда повреждаемый элемент поврежден
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsItemDamaged() => IsItemStackDamageable() && ItemDamage > 0;

        /// <summary>
        /// true, если этот стек предметов можно повредить
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsItemStackDamageable() => Item.MaxDamage > 0;

        /// <summary>
        /// Возвращает true, если стак может содержать 2 или более единиц элемента.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsStackable() 
            => Item.MaxStackSize > 1 && (!IsItemStackDamageable() || !IsItemDamaged());

        /// <summary>
        /// Сравнить предмет (стак без учёта количества)
        /// </summary>
        public bool IsItemEqual(ItemStack other) => other != null 
            && Item.IndexItem == other.Item.IndexItem && ItemDamage == other.ItemDamage;

        #endregion


        /// <summary>
        /// Записать стак в буффер пакета
        /// </summary>
        public static void WriteStream(ItemStack itemStack, WritePacket stream)
        {
            if (itemStack == null)
            {
                stream.Short(-1);
            }
            else
            {
                stream.Short((short)itemStack.Item.IndexItem);
                stream.Byte(itemStack.Amount);
                stream.Short((short)itemStack.ItemDamage);
            }
        }


        /// <summary>
        /// Прочесть стак с буфера пакета
        /// </summary>
        public static ItemStack ReadStream(ReadPacket stream)
        {
            int id = stream.Short();
            if (id >= 0)
            {
                byte amount = stream.Byte();
                int itemDamage = stream.Short();
                return new ItemStack(Ce.Items.ItemObjects[id], amount, itemDamage);
            }
            return null;
        }

        /// <summary>
        /// Записать стак в NBT
        /// </summary>
        public TagCompound WriteToNBT(TagCompound nbt)
        {
            nbt.SetShort("Id", (short)Item.IndexItem);
            nbt.SetByte("Amount", Amount);
            nbt.SetShort("Damage", (short)ItemDamage);
            return nbt;
        }


        /// <summary>
        /// Прочесть стак с NBT
        /// </summary>
        public static ItemStack ReadFromNBT(TagCompound nbt)
        {
            if (nbt.HasKey("Id"))
            { 
                return new ItemStack(Ce.Items.ItemObjects[nbt.GetShort("Id")], 
                    nbt.GetByte("Amount"), nbt.GetShort("Damage"));
            }
            return null;
        }

        public override string ToString() => Item.ToString() + " (" + Amount + ")";
    }
}
