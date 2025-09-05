using Vge.Item;
using Vge.NBT;
using Vge.Util;

namespace Vge.Entity
{
    public interface IInventory
    {
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

        void WriteToNBT(TagCompound nbt);

        void ReadFromNBT(TagCompound nbt);

        */
    }
}
