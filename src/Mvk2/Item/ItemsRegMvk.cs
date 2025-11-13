using System.Dynamic;
using Vge.Item;

namespace Mvk2.Item
{
    /// <summary>
    /// Регистрация предметов для Малювек 2
    /// </summary>
    public sealed class ItemsRegMvk
    {

        public static ItemBase AxeIron { get; private set; }
        public static ItemBase FlowerClover { get; private set; }
        public static ItemBase Cobblestone { get; private set; }
        public static ItemBase Brol { get; private set; }
        public static ItemBase Jeans { get; private set; }
        public static ItemBase StrawHat { get; private set; }
        public static ItemBase CamouflageJacket { get; private set; }
        public static ItemBase TacticalBelt { get; private set; }
        public static ItemBase Tie { get; private set; }

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

            ItemsReg.RegisterItemClass("AxeIron", AxeIron = new ItemBase());
            ItemsReg.RegisterItemClass("FlowerClover", FlowerClover = new ItemBase());
            ItemsReg.RegisterItemClass("Cobblestone", Cobblestone = new ItemBase());
            ItemsReg.RegisterItemClass("Brol", Brol = new ItemBase());
            ItemsReg.RegisterItemClass("Jeans", Jeans = new ItemCloth());
            ItemsReg.RegisterItemClass("StrawHat", StrawHat = new ItemCloth());
            ItemsReg.RegisterItemClass("CamouflageJacket", CamouflageJacket = new ItemCloth());
            ItemsReg.RegisterItemClass("TacticalBelt", TacticalBelt = new ItemCloth());
            ItemsReg.RegisterItemClass("Tie", Tie = new ItemCloth());
            
        }
    }
}
