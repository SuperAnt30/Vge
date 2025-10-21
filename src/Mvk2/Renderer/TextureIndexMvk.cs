using Vge.Renderer;

namespace Mvk2.Renderer
{
    /// <summary>
    /// Класс переменных индексов текстур для малювек
    /// </summary>
    public class TextureIndexMvk : TextureIndex
    {
        /// <summary>
        /// Текстура большого окна 512х416
        /// </summary>
        public uint WindowBig;
        /// <summary>
        /// Текстура малого окна 320х200
        /// </summary>
        public uint WindowSmall;
        /// <summary>
        /// Текстура чат окна
        /// </summary>
        public uint Chat;
        /// <summary>
        /// Текстура Heads-Up Display
        /// </summary>
        public uint Hud;
        /// <summary>
        /// Текстура Inventory
        /// </summary>
        public uint Inventory;
        /// <summary>
        /// Текстура контейнера хранилища Storage
        /// </summary>
        public uint ConteinerStorage;
        
    }


    /// <summary>
    /// Перечисление разных ключей текстур для малювек
    /// </summary>
    public enum EnumTextureMvk
    {
        /// <summary>
        /// Мелкий шрифт (8 пиксель)
        /// </summary>
        FontSmall,
        /// <summary>
        /// Крупный шрифт (16 пиксель)
        /// </summary>
        FontLarge,
        /// <summary>
        /// Большое окно 512х416
        /// </summary>
        WindowBig,
        /// <summary>
        /// Малое окно 320х200
        /// </summary>
        WindowSmall,
        /// <summary>
        /// Чат окно
        /// </summary>
        Chat,
        /// <summary>
        /// Heads-Up Display
        /// </summary>
        Hud,
        /// <summary>
        /// Текстура Inventory
        /// </summary>
        Inventory,
        /// <summary>
        /// Текстура контейнера хранилища Storage
        /// </summary>
        ConteinerStorage
    }

}
