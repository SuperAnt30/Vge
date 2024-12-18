namespace Vge.Renderer.Font
{
    /// <summary>
    /// Объект символа
    /// </summary>
    public struct Symbol
    {
        /// <summary>
        /// Символ
        /// </summary>
        public readonly char Symb;
        /// <summary>
        /// Ширина символа
        /// </summary>
        public readonly byte Width;

        public readonly float V1;
        public readonly float V2;
        public readonly float U1;
        public readonly float U2;

        public Symbol(char c, int index, byte width)
        {
            Symb = c;

            V1 = (index >> 4) * .0625f;
            V2 = V1 + .0625f;
            U1 = (index & 15) * .0625f;
            U2 = U1 + .0625f;

            Width = width;
        }

        #region Style

        /// <summary>
        /// Является ли символ Амперсанд § (Alt+21)
        /// </summary>
        public bool IsAmpersand() => Symb == '§';
        /// <summary>
        /// Ресет
        /// </summary>
        public bool IsReset() => Symb == 'r';
        /// <summary>
        /// Жирный
        /// </summary>
        public bool IsBolb() => Symb == 'l';
        /// <summary>
        /// Наклонный
        /// </summary>
        public bool IsItalic() => Symb == 'o';
        /// <summary>
        /// Подчеркнутый
        /// </summary>
        public bool IsUnderline() => Symb == 'n';
        /// <summary>
        /// Зачеркнутый
        /// </summary>
        public bool IsStrikethrough() => Symb == 'm';

        /// <summary>
        /// Получить индекс цвета 0-15, если 255, то это не цвет
        /// </summary>
        public byte GetIndexColor()
        {
            switch (Symb)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'a': case 'A': return 10;
                case 'b': case 'B': return 11;
                case 'c': case 'C': return 12;
                case 'd': case 'D': return 13;
                case 'e': case 'E': return 14;
                case 'f': case 'F': return 15;
            }
            return 255;
        }

        #endregion

        public override string ToString()
            => Symb + " " + (int)Symb;
    }
}
