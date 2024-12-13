namespace Vge.Renderer
{
    /// <summary>
    /// Класс переменных индексов текстур
    /// </summary>
    public class TextureIndex
    {
        /// <summary>
        /// Текстура загрузки
        /// </summary>
        public uint Splash;
        /// <summary>
        /// Текстура основного виджета
        /// </summary>
        public uint Widgets;
        /// <summary>
        /// Текстура атласа размытых блоков
        /// </summary>
        public uint AtlasBlurry;
        /// <summary>
        /// Текстура атласа блоков с чёткой резкостью
        /// </summary>
        public uint AtlasSharpness;
    }

    /// <summary>
    /// Перечисление разных ключей текстур
    /// </summary>
    public enum EnumTexture
    {
        /// <summary>
        /// Шрифт стандартный
        /// </summary>
        FontMain,
        /// <summary>
        /// Основной виджет
        /// </summary>
        Widgets
    }
}
