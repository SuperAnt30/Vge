namespace Vge.Renderer.Font
{
    /// <summary>
    /// Класс стиля шрифта
    /// § (Alt+21)
    /// </summary>
    public class FontStyle
    {
        /// <summary>
        /// Цвет шрифта §0..f
        /// </summary>
        private byte color = 255;
        /// <summary>
        /// Выделен ли шрифт §l
        /// </summary>
        private bool bolb = false;
        /// <summary>
        /// Наклонный шрифт §o
        /// </summary>
        private bool italic = false;
        /// <summary>
        /// Подчеркнутый шрифт §n
        /// </summary>
        private bool underline = false;
        /// <summary>
        /// Зачеркнутый шрифт §m
        /// </summary>
        private bool strikethrough = false;
        
        /// <summary>
        /// Выделен ли шрифт §l
        /// </summary>
        public bool IsBolb() => bolb;
        /// <summary>
        /// Наклонный ли шрифт §o
        /// </summary>
        public bool IsItalic() => italic;
        /// <summary>
        /// Подчеркнутый ли шрифт §n
        /// </summary>
        public bool IsUnderline() => underline;
        /// <summary>
        /// Зачеркнутый ли шрифт §m
        /// </summary>
        public bool IsStrikethrough() => strikethrough;
        /// <summary>
        /// Имеется ли цвет
        /// </summary>
        public bool IsColor() => color != 255;
        /// <summary>
        /// Получить индекс цвета
        /// </summary>
        public byte GetColor() => color;

        /// <summary>
        /// Выделен шрифт §l
        /// </summary>
        public void Bolb() => bolb = true;
        /// <summary>
        /// Наклонный шрифт §o
        /// </summary>
        public void Italic() => italic = true;
        /// <summary>
        /// Подчеркнутый шрифт §n
        /// </summary>
        public void Underline() => underline = true;
        /// <summary>
        /// Зачеркнутый шрифт §m
        /// </summary>
        public void Strikethrough() => strikethrough = true;

        public void SetSymbol(Symbol symbol)
        {
            if (symbol.IsReset())
            {
                // Ресет
                Reset();
            }
            else if (symbol.IsBolb())
            {
                // Жирный
                bolb = true;
            }
            else if (symbol.IsItalic())
            {
                // Наклонный
                italic = true;
            }
            else if (symbol.IsUnderline())
            {
                // Подчеркнутый
                underline = true;
            }
            else if (symbol.IsStrikethrough())
            {
                // Зачеркнутый
                strikethrough = true;
            }
            else
            {
                // Получить цвет
                color = symbol.GetIndexColor();
            }
        }

        /// <summary>
        /// Сбросить параметры цвета
        /// </summary>
        public void Reset()
        {
            bolb = false;
            italic = false;
            strikethrough = false;
            underline = false;
            color = 255;
        }
    }
}
