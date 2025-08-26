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
            ItemsReg.RegisterItemClass("FlowerClover", new ItemBase());
            ItemsReg.RegisterItemClass("Cobblestone", new ItemBase());
            ItemsReg.RegisterItemClass("Brol", new ItemBase());
        }
    }
}
