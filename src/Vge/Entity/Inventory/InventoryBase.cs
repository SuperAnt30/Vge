using System.Collections.Generic;
using Vge.Item;
using Vge.NBT;
using Vge.Util;

namespace Vge.Entity.Inventory
{
    /// <summary>
    /// Базовый класс инвенторя
    /// </summary>
    public class InventoryBase
    {

        public void Clear()
        {
            //int i;
            //for (i = 0; i < mainInventory.Length; i++)
            //{
            //    mainInventory[i] = null;
            //}
            //for (i = 0; i < mainBackpack.Length; i++)
            //{
            //    mainBackpack[i] = null;
            //}
            //for (i = 0; i < clothInventory.Length; i++)
            //{
            //    clothInventory[i] = null;
            //}
            //OnChanged(-1);
        }
        /*
        /// <summary>
        /// Получить выбранный стак правой руки
        /// </summary>
        ItemStack GetCurrentItem();

        /// <summary>
        /// Возвращает стaк в слоте slotIn
        /// </summary>
        ItemStack GetStackInSlot(int slotIn);

        /// <summary>
        /// Получить стак одежды
        /// </summary>
        /// <param name="slot">0-7 InventoryPlayer.COUNT_CLOTH</param>
        ItemStack GetClothInventory(int slot);

        /// <summary>
        /// Получить список стаков (что в руке и список одежды)
        /// </summary>
        ItemStack[] GetCurrentItemAndCloth();

        /// <summary>
        /// Задать в правую руку стак
        /// </summary>
        void SetCurrentItem(ItemStack stack);

        /// <summary>
        /// Задать стак одежды
        /// </summary>
        /// <param name="slot">0-7 InventoryPlayer.COUNT_CLOTH</param>
        void SetClothInventory(int slot, ItemStack stack);

        /// <summary>
        /// Устанавливает данный стек предметов в указанный слот в инвентаре
        /// </summary>
        void SetInventorySlotContents(int slotIn, ItemStack stack);

        /// <summary>
        /// Задать список стаков (что в руке и список одежды)
        /// </summary>
        void SetCurrentItemAndCloth(ItemStack[] stacks);

        /// <summary>
        /// Уменьшает урон, в зависимости от брони
        /// </summary>
        float ApplyArmorCalculations(EnumBodyDamage enumBody, float amount);

        /// <summary>
        /// Уменьшает урон, в зависимости от магии
        /// </summary>
        float ApplyMagicDamageCalculations(EnumDamageSource source, EnumBodyDamage enumBody, float amount);

        /// <summary>
        /// Получить урон для атаки предметом который в руке со всеми нюансами игрока
        /// </summary>
        float GetDamageToAttack(float damage);

        /// <summary>
        /// Дроп одежды и или инвентаря
        /// </summary>
        void DropEquipment(Rand rand);
        /// <summary>
        /// Дроп одежды и или инвентаря
        /// </summary>
        void DropEquipment(int index);
        */

        /// <summary>
        /// Получить полный список всего инвентаря
        /// Mvk было GetMainAndCloth
        /// </summary>
        public Slot[] GetAll()
        {
            return new Slot[] { new Slot() };
        }

        /// <summary>
        /// Устанавливает данный стак предметов в указанный слот в инвентаре 
        /// </summary>
        public void SetInventorySlotContents(Slot slot)
        {

        }

        public virtual void WriteToNBT(TagCompound nbt)
            => NBTTools.ItemStacksWriteToNBT(nbt, "Inventory", GetAll());

        public virtual void ReadFromNBT(TagCompound nbt)
        {
            Clear();
            Slot[] slots = NBTTools.ItemStacksReadFromNBT(nbt, "Inventory");
            foreach (Slot slot in slots)
            {
                SetInventorySlotContents(slot);
            }
        }

        
    }
}
