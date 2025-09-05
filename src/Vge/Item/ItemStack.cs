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
        public int Amount { get; private set; }

        public ItemStack(ItemBase item, int amount, int itemDamage)
        {
            Item = item;
            Amount = amount;
            ItemDamage = itemDamage;
        }
        public ItemStack(ItemBase item, int amount)
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
                stream.Byte((byte)itemStack.Amount);
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
                int amount = stream.Byte();
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
            nbt.SetByte("Amount", (byte)Amount);
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
    }
}
