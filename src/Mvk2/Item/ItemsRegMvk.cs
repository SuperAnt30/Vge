using Vge.Item;

namespace Mvk2.Item
{
    /// <summary>
    /// Регистрация предметов для Малювек 2
    /// </summary>
    public sealed class ItemsRegMvk
    {
        public static void Initialization()
        {
            ItemsReg.RegisterItemClass("AxeIron", new ItemBase());
            ItemsReg.RegisterItemClass("FlowerClover", new ItemBase());
            ItemsReg.RegisterItemClass("Cobblestone", new ItemBase());
            ItemsReg.RegisterItemClass("Brol", new ItemBase());
            ItemsReg.RegisterItemClass("Jeans", new ItemCloth());
            ItemsReg.RegisterItemClass("StrawHat", new ItemCloth());
            ItemsReg.RegisterItemClass("Bracelet", new ItemCloth());
            ItemsReg.RegisterItemClass("Bracelet2", new ItemCloth());
        }
    }
}
