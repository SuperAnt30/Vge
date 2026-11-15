using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Item;
using Vge.World.BlockEntity;

namespace Mvk2.Item
{
    /// <summary>
    /// Объект регистрации и данных креативного инвентаря
    /// </summary>
    public sealed class ItemsCreative : IBlockStorage
    {
        /// <summary>
        /// Количество ячеек в одной закладке
        /// </summary>
        public const int Count = 48;
        /// <summary>
        /// Количество вкладок
        /// All, Blocks, ItemsForBlock, Craft, Tools, Cloth, Food
        /// </summary>
        public const int CountTab = 7;
        /// <summary>
        /// Массив названий разделов
        /// </summary>
        public readonly static string[] NameTab = new string[]
        {
            L.T("All"), L.T("Blocks"), L.T("ItemsForBlock"), L.T("Craft"), L.T("Tools"), L.T("Cloth"), L.T("Food")
        };

        private ItemStack[][][] _stacks;

        /// <summary>
        /// Объект регистрации и данных креативного инвентаря
        /// </summary>
        public ItemsCreative()
        {
            // Раздел блоки
            List<ItemStack> tabBlocks = new List<ItemStack>();
            tabBlocks.Add(new ItemStack(ItemsRegMvk.Debug));
            tabBlocks.Add(new ItemStack(ItemsRegMvk.Water));
            tabBlocks.Add(new ItemStack(ItemsRegMvk.Lava));
            tabBlocks.Add(new ItemStack(ItemsRegMvk.Cobblestone));
            tabBlocks.Add(new ItemStack(ItemsRegMvk.Sand));
            tabBlocks.Add(new ItemStack(ItemsRegMvk.Glass));
            tabBlocks.Add(new ItemStack(ItemsRegMvk.Brol));

            // Раздел предметы для блока
            List<ItemStack> tabItemsForBlock = new List<ItemStack>();
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.FlowerClover));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.DoorOfPlanks));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.Rope));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.Bucket));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.BucketWater, 1, 0, 7));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.BucketLava, 1, 0, 7));

            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.FetusBirch));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.SaplingBirch));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.FetusConifer));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.SaplingConifer));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.FetusFruit));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.SaplingFruit));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.FetusOak));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.SaplingOak));
            tabItemsForBlock.Add(new ItemStack(ItemsRegMvk.SaplingDry));

            // Раздел крафтовые предметы 
            List<ItemStack> tabCraft = new List<ItemStack>();
            tabCraft.Add(new ItemStack(ItemsRegMvk.PieceStone));
            tabCraft.Add(new ItemStack(ItemsRegMvk.PieceDirt));
            tabCraft.Add(new ItemStack(ItemsRegMvk.DryGrass));

            // Раздел инструменты и оружие
            List<ItemStack> tabTools = new List<ItemStack>();
            tabTools.Add(new ItemStack(ItemsRegMvk.StickTechnical));
            tabTools.Add(new ItemStack(ItemsRegMvk.AxeStone));
            tabTools.Add(new ItemStack(ItemsRegMvk.DiggingStick));
            tabTools.Add(new ItemStack(ItemsRegMvk.AxeIron));
            tabTools.Add(new ItemStack(ItemsRegMvk.ShovelIron));
            tabTools.Add(new ItemStack(ItemsRegMvk.PickaxeIron));
            tabTools.Add(new ItemStack(ItemsRegMvk.AxeSteel));
            tabTools.Add(new ItemStack(ItemsRegMvk.ShovelSteel));
            tabTools.Add(new ItemStack(ItemsRegMvk.PickaxeSteel));
            tabTools.Add(new ItemStack(ItemsRegMvk.AxeRed));
            tabTools.Add(new ItemStack(ItemsRegMvk.ShovelRed));
            tabTools.Add(new ItemStack(ItemsRegMvk.PickaxeRed));


            // Раздел одежда
            List<ItemStack> tabCloth = new List<ItemStack>();
            
            tabCloth.Add(new ItemStack(ItemsRegMvk.Tie));
            tabCloth.Add(new ItemStack(ItemsRegMvk.ShirtBranded));
            tabCloth.Add(new ItemStack(ItemsRegMvk.BootsBranded));
            tabCloth.Add(new ItemStack(ItemsRegMvk.BackpackBranded));
            tabCloth.Add(new ItemStack(ItemsRegMvk.PantsJeans));
            tabCloth.Add(new ItemStack(ItemsRegMvk.BeltBranded));
            tabCloth.Add(new ItemStack(ItemsRegMvk.CapDark));

            tabCloth.Add(new ItemStack(ItemsRegMvk.StrawHat));
            tabCloth.Add(new ItemStack(ItemsRegMvk.CamouflageJacket));

            // Раздел еда
            List<ItemStack> tabFood = new List<ItemStack>();
            // tabFood.Add(new ItemStack(ItemsRegMvk.AxeIron, 1, ItemsRegMvk.AxeIron.MaxDamage));
            tabFood.Add(new ItemStack(ItemsRegMvk.CannedFood));

            // Раздел всего
            List<ItemStack> tabAll = new List<ItemStack>();
            tabAll.AddRange(tabBlocks);
            tabAll.AddRange(tabItemsForBlock);
            tabAll.AddRange(tabCraft);
            tabAll.AddRange(tabTools);
            tabAll.AddRange(tabCloth);
            tabAll.AddRange(tabFood);

            _stacks = new ItemStack[CountTab][][];
            _Init(0, tabAll);
            _Init(1, tabBlocks);
            _Init(2, tabItemsForBlock);
            _Init(3, tabCraft);
            _Init(4, tabTools);
            _Init(5, tabCloth);
            _Init(6, tabFood);

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
