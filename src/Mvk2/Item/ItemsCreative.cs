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
        /// All, Blocks, Craft, Tools, Cloth, Food
        /// </summary>
        public const int CountTab = 6;
        /// <summary>
        /// Массив названий разделов
        /// </summary>
        public readonly static string[] NameTab = new string[]
        {
            L.T("All"), L.T("Blocks"), L.T("Craft"), L.T("Tools"), L.T("Cloth"), L.T("Food")
        };

        private ItemStack[][][] _stacks;

        public ItemsCreative()
        {
            // Раздел блоки
            List<ItemStack> tabBlocks = new List<ItemStack>();
            tabBlocks.Add(new ItemStack(ItemsRegMvk.Cobblestone));
            tabBlocks.Add(new ItemStack(ItemsRegMvk.Brol));

            // Раздел крафтовые предметы 
            List<ItemStack> tabCraft = new List<ItemStack>();
            tabCraft.Add(new ItemStack(ItemsRegMvk.FlowerClover));

            // Раздел инструменты и оружие
            List<ItemStack> tabTools = new List<ItemStack>();
            tabTools.Add(new ItemStack(ItemsRegMvk.AxeIron, 1, ItemsRegMvk.AxeIron.MaxDamage));

            // Раздел одежда
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

            // Раздел еда
            List<ItemStack> tabFood = new List<ItemStack>();
           // tabFood.Add(new ItemStack(ItemsRegMvk.AxeIron, 1, ItemsRegMvk.AxeIron.MaxDamage));

            // Раздел всего
            List<ItemStack> tabAll = new List<ItemStack>();
            tabAll.AddRange(tabBlocks);
            tabAll.AddRange(tabCraft);
            tabAll.AddRange(tabTools);
            tabAll.AddRange(tabCloth);
            tabAll.AddRange(tabFood);

            _stacks = new ItemStack[CountTab][][];
            _Init(0, tabAll);
            _Init(1, tabBlocks);
            _Init(2, tabCraft);
            _Init(3, tabTools);
            _Init(4, tabCloth);
            _Init(5, tabFood);

            return;
        }

        private void _Init(int index, List<ItemStack> tab)
        {
            int count = (tab.Count + Count - 1) / Count;
            if (count < 1) count = 1;
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
