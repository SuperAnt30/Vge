using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Network.Packets.Server;
using Vge.World.BlockEntity;

namespace Mvk2.Item
{
    /// <summary>
    /// Объект регистрации и данных креативногоинвентаря
    /// </summary>
    public sealed class ItemsCreative : IBlockStorage
    {
        /// <summary>
        /// Количество ячеек в одной закладке
        /// </summary>
        public const int Count = 48;
        /// <summary>
        /// Количество вкладок
        /// </summary>
        public const int CountTab = 4;

        private ItemStack[][][] _stacks;

        public ItemsCreative()
        {
            // Раздел первый блоки
            List<ItemStack> tabCube = new List<ItemStack>();
            tabCube.Add(new ItemStack(ItemsRegMvk.Cobblestone));
            tabCube.Add(new ItemStack(ItemsRegMvk.Brol));

            // Раздел второй одежда
            List<ItemStack> tabCloth = new List<ItemStack>();
            
            tabCloth.Add(new ItemStack(ItemsRegMvk.Tie, 1, ItemsRegMvk.Tie.MaxDamage));
            tabCloth.Add(new ItemStack(ItemsRegMvk.ShirtBranded, 1, ItemsRegMvk.ShirtBranded.MaxDamage));
            tabCloth.Add(new ItemStack(ItemsRegMvk.BootsBranded, 1, ItemsRegMvk.BootsBranded.MaxDamage));
            tabCloth.Add(new ItemStack(ItemsRegMvk.BackpackBranded, 1, ItemsRegMvk.BackpackBranded.MaxDamage));
            tabCloth.Add(new ItemStack(ItemsRegMvk.PantsJeans, 1, ItemsRegMvk.PantsJeans.MaxDamage));
            tabCloth.Add(new ItemStack(ItemsRegMvk.BeltBranded, 1, ItemsRegMvk.BeltBranded.MaxDamage));
            tabCloth.Add(new ItemStack(ItemsRegMvk.CapDark, 1, ItemsRegMvk.CapDark.MaxDamage));

            tabCloth.Add(new ItemStack(ItemsRegMvk.StrawHat, 1, ItemsRegMvk.StrawHat.MaxDamage));
            tabCloth.Add(new ItemStack(ItemsRegMvk.CamouflageJacket, 1, ItemsRegMvk.CamouflageJacket.MaxDamage));

            tabCloth.Add(new ItemStack(ItemsRegMvk.AxeIron, 1, ItemsRegMvk.AxeIron.MaxDamage));

            // Раздел третий 
            List<ItemStack> tab3 = new List<ItemStack>();
            tab3.Add(new ItemStack(ItemsRegMvk.FlowerClover));

            // Раздел всего
            List<ItemStack> tabAll = new List<ItemStack>();
            tabAll.AddRange(tabCube);
            tabAll.AddRange(tabCloth);
            tabAll.AddRange(tab3);

            _stacks = new ItemStack[CountTab][][];
            _Init(0, tabAll);
            _Init(1, tabCube);
            _Init(2, tabCloth);
            _Init(3, tab3);
        }

        private void _Init(int index, List<ItemStack> tab)
        {
            int count = (tab.Count + Count - 1) / Count;
            _stacks[index] = new ItemStack[count][];
            for (int i = 0; i < count; i++)
            {
                _stacks[index][i] = new ItemStack[Count];
            }
            // Заполняем
            int j, k;
            for (int i = 0; i < tab.Count; i++)
            {
                j = i  / Count;
                k = i - j * Count;
                _stacks[index][j][k] = tab[i];
            }
        }

        /// <summary>
        /// Получить количество страниц в закладке
        /// </summary>
        public int GetCountPage(int tab) => _stacks[tab].Length;

        /// <summary>
        /// Получить стак в конкретном слоте конкретной закладки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemStack GetStackInSlot(int tab, int page, int slotIn) 
            => _stacks[tab][page][slotIn];

        /// <summary>
        /// Добавляет стак предметов в инвентарь, возвращает false, если это невозможно.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddItemStackToInventory(ItemStack stack) => CanPutItemStack(0, stack);

        /// <summary>
        /// Проверяем можно ли установить данный стак в определённой ячейке склада
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanPutItemStack(int slotIn, ItemStack stack) => stack != null;

        /// <summary>
        /// Проверить равенства
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckEquals(IBlockStorage blockStorage) => false;

        /// <summary>
        /// Получить стак в конкретном слоте
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemStack GetStackInSlot(int slotIn) => null;

        /// <summary>
        /// Открыли окно, вызывается объектом EntityPlayerServer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OpenWindow(PlayerServer entityPlayer) { }

        /// <summary>
        /// Задать стак в конкретном слоте
        /// </summary>
        /// <param name="isModify">Отметить что было изменение, для перезаписи на сервере в файл сохранения</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStackInSlot(int slotIn, ItemStack stack) { }
    }
}
