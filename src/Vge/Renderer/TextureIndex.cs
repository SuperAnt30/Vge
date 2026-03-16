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
        /// Текстура частичек
        /// </summary>
        public uint Particles;
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
        Widgets,
        /// <summary>
        /// Текстура частичек
        /// </summary>
        Particles
    }
}
