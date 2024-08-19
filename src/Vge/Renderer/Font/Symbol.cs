using System;
using WinGL.Util;

namespace Vge.Renderer.Font
{
    /// <summary>
    /// Объект символа
    /// </summary>
    public struct Symbol
    {
        /// <summary>
        /// Размер шрифта
        /// </summary>
        public byte Size { get; private set; }
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

        public Symbol(char c, byte size, BufferedImage bi)
        {
            Symb = c;

            int index = FontAdvance.Key.IndexOf(Symb) + 32;
            if (index == -1)
            {
                throw new Exception("Симыол [" + c + "] отсутствует в перечне.");
            }
            Size = size;

            U1 = (index & 15) * 0.0625f;
            U2 = U1 + 0.0625f;
            V1 = (index >> 4) * 0.0625f;
            V2 = V1 + 0.0625f;

            Width = 0;
            Width = GetWidth(bi, index);
        }


        /// <summary>
        /// Получить ширину символа
        /// </summary>
        private byte GetWidth(BufferedImage bi, int index)
        {
            int advance = FontAdvance.HoriAdvance[Size];

            int x0 = (index & 15) * advance;
            int y0 = (index >> 4) * advance;
            int x1 = x0 + advance - 1;
            int y1 = y0 + advance;

            for (int x = x1; x >= x0; x--)
            {
                for (int y = y0; y < y1; y++)
                {
                    if (bi.GetPixelAlpha(x, y) > 0)
                    {
                        return (byte)(x - x0 + 1);
                    }
                }
            }
            return 4;
        }

        public override string ToString()
            => Symb + " " + (int)Symb;
    }
}
