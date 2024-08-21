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
        public char Symb { get; private set; }
        /// <summary>
        /// Ширина символа
        /// </summary>
        public byte Width { get; private set; }

        public float U1 { get; private set; }
        public float U2 { get; private set; }
        public float V1 { get; private set; }
        public float V2 { get; private set; }

        public Symbol(char c, int index, byte width)
        {
            Symb = c;

            U1 = (index & 15) * 0.0625f;
            U2 = U1 + 0.0625f;
            V1 = (index >> 4) * 0.0625f;
            V2 = V1 + 0.0625f;

            Width = width;
        }

        public override string ToString()
            => Symb + " " + (int)Symb;
    }
}
