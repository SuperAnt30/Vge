namespace Vge.Item
{
    /// <summary>
    /// Константы названий тег для параметров предметов в json
    /// </summary>
    public sealed class Cti
    {
        /// <summary>
        /// Имя строки к текстуре, спрайта 2д. (string)
        /// </summary>
        public const string Sprite = "Sprite";
        /// <summary>
        /// Имя к объекту фигуры предмета. (string)
        /// </summary>
        public const string Shape = "Shape";
        /// <summary>
        /// Имя к объекту фигуры блока. (string)
        /// </summary>
        public const string ShapeBlock = "ShapeBlock";
        /// <summary>
        /// Имя строки к текстуре для Gui, спрайта 2д. (string)
        /// </summary>
        public const string SpriteGui = "SpriteGui";
        /// <summary>
        /// Имя к объекту фигуры предмета для Gui. (string)
        /// </summary>
        public const string ShapeGui = "ShapeGui";
        /// <summary>
        /// Имя к объекту фигуры блока для Gui. (string)
        /// </summary>
        public const string ShapeBlockGui = "ShapeBlockGui";
        /// <summary>
        /// Имя объекта атрибут для внешнего вида фигуры. Это аналог данных из массива Variants из блока ({})
        /// </summary>
        public const string View = "View";

        /// <summary>
        /// Имя к максимальному количеству однотипный вещей в одной ячейке. (int)
        /// </summary>
        public const string MaxStackSize = "MaxStackSize";
        /// <summary>
        /// Имя к максимальному количеству урона, при 0, нет учёта урона. (int)
        /// </summary>
        public const string MaxDamage = "MaxDamage";

    }
}
