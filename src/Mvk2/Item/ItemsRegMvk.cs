using Mvk2.Item.List;
using Mvk2.Item.List.Tool;
using Mvk2.World.Block;
using Vge.Item;
using Vge.Item.List;

namespace Mvk2.Item
{
    /// <summary>
    /// Регистрация предметов для Малювек 2
    /// </summary>
    public sealed class ItemsRegMvk
    {
        #region Одежда
        /// <summary>
        /// Галстук
        /// </summary>
        public static ItemCloth Tie { get; private set; }
        /// <summary>
        /// Фирменная рубашка
        /// </summary>
        public static ItemCloth ShirtBranded { get; private set; }
        /// <summary>
        /// Фирменные ботинки
        /// </summary>
        public static ItemCloth BootsBranded { get; private set; }
        /// <summary>
        /// Фирменный рюкзак
        /// </summary>
        public static ItemCloth BackpackBranded { get; private set; }
        /// <summary>
        /// Джинсовые брюки
        /// </summary>
        public static ItemCloth PantsJeans { get; private set; }
        /// <summary>
        /// Фирменный ремень
        /// </summary>
        public static ItemCloth BeltBranded { get; private set; }
        /// <summary>
        /// Кепка тёмная
        /// </summary>
        public static ItemCloth CapDark { get; private set; }

        #endregion

        #region Tools

        /// <summary>
        /// Техническая палочка, для отладки
        /// </summary>
        public static ItemStickTechnical StickTechnical { get; private set; }


        /// <summary>
        /// Каменный топор
        /// </summary>
        public static ItemAbTool AxeStone { get; private set; }
        /// <summary>
        /// Железный топор
        /// </summary>
        public static ItemAbTool AxeIron { get; private set; }
        /// <summary>
        /// Железная лопата
        /// </summary>
        public static ItemAbTool ShovelIron { get; private set; }
        /// <summary>
        /// Железная кирка
        /// </summary>
        public static ItemAbTool PickaxeIron { get; private set; }
        /// <summary>
        /// Стальной топор
        /// </summary>
        public static ItemAbTool AxeSteel { get; private set; }
        /// <summary>
        /// Стальная лопата
        /// </summary>
        public static ItemAbTool ShovelSteel { get; private set; }
        /// <summary>
        /// Стальная кирка
        /// </summary>
        public static ItemAbTool PickaxeSteel { get; private set; }
        /// <summary>
        /// Красный фирменный топор
        /// </summary>
        public static ItemAbTool AxeRed { get; private set; }

        #endregion

        /// <summary>
        /// Консерва
        /// </summary>
        public static ItemBase CannedFood { get; private set; }
        /// <summary>
        /// Сухая трава
        /// </summary>
        public static ItemBase DryGrass { get; private set; }
        /// <summary>
        /// Земляной кусочек
        /// </summary>
        public static ItemBase PieceDirt { get; private set; }
        /// <summary>
        /// Каменный кусочек
        /// </summary>
        public static ItemBase PieceStone { get; private set; }
        /// <summary>
        /// Цветок клевер белый
        /// </summary>
        public static ItemBase FlowerClover { get; private set; }
        public static ItemBase Cobblestone { get; private set; }
        public static ItemBase Brol { get; private set; }
        public static ItemBase Debug { get; private set; }

        public static ItemCloth StrawHat { get; private set; }
        public static ItemCloth CamouflageJacket { get; private set; }

        /// <summary>
        /// Объект креативного инвентаря
        /// </summary>
        public static ItemsCreative Creative;

        public static void Initialization()
        {
            // В ядре нет регистрации обязательный предметов, только тут
            // TODO::!!!
            // Br - Браконьер, Bu - Бедуин, P - Игрок старт

            // === Одежда
            // - Шапка
            // Cap - Кепка (можно сделать несколько видов, текстур) [P]
            // Keffiyeh - Куфия [Bu]
            // BeretBlack - Чёрный берет [Br]
            // // DiverMask - Маска дайвера
            // StrawHat - Соломенная шляпа, крафт
            // LeatherHat - Кожаная шляпа, крафт

            // - Торс
            // Shirt - Рубашка (1) [P] 3
            // Tunic - Туника (1) [Bu] попробовать сделать вместе с ногами  3
            // CamouflageJacket - Куртка камуфляж (4) [Br] 5
            // JacketOfRags - Куртка из тряпок, крафт (1) 3
            // LeatherJacket - Кожаная куртка, крафт (2) 5

            // - Пояс
            // BrandedBelt - Фирменный ремень (2) [P]
            // TacticalBelt - Ремень тактический (2) [Br]
            // ClothBelt - Пояс тряпичный, крафт (1)
            // LeatherBelt - Кожаный ремень, крафт (2)

            // - Штаны
            // Jeans - Джинсы (4) [P]
            // Shorts - Шорты, крафт* (2) [P]
            // CamouflagePants - Брюки камуфляж (6) [Br]
            // StrawSkirt - Соломенная юбка, крафт (1)
            // LeatherPants - Кожаные штаны, крафт (4)

            // - Обувь
            // BrandedBelt - Фирменный ботинки [P]
            // TacticalBoots - Ботинки тактические [Br]
            // Shale - Сланцы [P, Bu]
            // BastShoes - Лапти, крафт
            // LeatherBoots - Ботинки кожаные, крафт [M]

            // - Руки
            // WristWatch - Наручные часы (Время)
            // SmartWatch - Смарт часы (Координаты и время)
            // ToolDropProtection - Защита от падения инструмента
            // FortuneBracelet - Браслет удачи (выподает дроп)
            // PowerBracelet - Браслет силы (быстре ломает)
            // BraceletSpeed - Браслет скорости (быстре перемещается)
            // BraceletAgility - Браслет ловкости (падение перса)

            // - Шея
            // Tablet - Планшет (Мини карта с координатами)
            // Tie - Галстук 
            // MascotDestroy - Талисман крушить (на части)
            // MascotCarefully - Талисман осторожно (шёлк)
            // MascotSpeed - Талисман скорости (быстре перемещается)

            // - Рюкзак
            // TacticalBackpack - Рюкзак тактический [Br] (35)
            // TouristBackpack - Рюкзак туристический (35)
            // LeatherBackpack - Рюкзак кожаный, крафт (28)
            // ClothBackpack - Рюкзак тряпичный, крафт (21)
            // StrawBackpack - Рюкзак из соломы, крафт (14)

            // - Глаза **
            // Очки, декор
            // Солнечные очки, декор + темнее
            // Очки ночного видения
            // Маска + Акваланг ?!
            // 


            // Tools
            ItemsReg.RegisterItemClass("StickTechnical", StickTechnical = new ItemStickTechnical(), "Tools");

            
            ItemsReg.RegisterItemClass("AxeStone", AxeStone = new ItemAxe(), "Tools");
            ItemsReg.RegisterItemClass("AxeIron", AxeIron = new ItemAxe(), "Tools");
            ItemsReg.RegisterItemClass("AxeSteel", AxeSteel = new ItemAxe(), "Tools");
            ItemsReg.RegisterItemClass("AxeRed", AxeRed = new ItemAxe(), "Tools");

            ItemsReg.RegisterItemClass("ShovelIron", ShovelIron = new ItemShovel(), "Tools");
            ItemsReg.RegisterItemClass("ShovelSteel", ShovelSteel = new ItemShovel(), "Tools");

            ItemsReg.RegisterItemClass("PickaxeIron", PickaxeIron = new ItemPickaxe(), "Tools");
            ItemsReg.RegisterItemClass("PickaxeSteel", PickaxeSteel = new ItemPickaxe(), "Tools");
            

            ItemsReg.RegisterItemClass("CannedFood", CannedFood = new ItemFood());
            ItemsReg.RegisterItemClass("DryGrass", DryGrass = new ItemBase());
            ItemsReg.RegisterItemClass("PieceDirt", PieceDirt = new ItemSpawn());
            ItemsReg.RegisterItemClass("PieceStone", PieceStone = new ItemPiece());

            ItemsReg.RegisterItemClass("FlowerClover", FlowerClover = new ItemBlock(BlocksRegMvk.SaplingOak));

            ItemsReg.RegisterItemClass("Cobblestone", Cobblestone = new ItemBlock(BlocksRegMvk.Cobblestone));
            ItemsReg.RegisterItemClass("Brol", Brol = new ItemBlock(BlocksRegMvk.Brol));
            ItemsReg.RegisterItemClass("Debug", Debug = new ItemBlock(BlocksRegMvk.Debug));

            StrawHat = ItemsReg.RegisterItemClothClass("StrawHat");
            CamouflageJacket = ItemsReg.RegisterItemClothClass("CamouflageJacket");

            PantsJeans = ItemsReg.RegisterItemClothClass("PantsJeans");
            ShirtBranded = ItemsReg.RegisterItemClothClass("ShirtBranded");
            BootsBranded = ItemsReg.RegisterItemClothClass("BootsBranded");
            BackpackBranded = ItemsReg.RegisterItemClothClass("BackpackBranded");
            BeltBranded = ItemsReg.RegisterItemClothClass("BeltBranded");
            Tie = ItemsReg.RegisterItemClothClass("Tie");
            CapDark = ItemsReg.RegisterItemClothClass("CapDark");

            // Объект регистрации и данных креативного инвентаря
            Creative = new ItemsCreative();
        }
    }
}
