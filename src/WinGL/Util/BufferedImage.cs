namespace WinGL.Util
{
    /// <summary>
    /// Объект изображения
    /// </summary>
    public struct BufferedImage
    {
        /// <summary>
        /// Ширина
        /// </summary>
        public int width;
        /// <summary>
        /// Высотп
        /// </summary>
        public int height;
        /// <summary>
        /// Массив байт
        /// </summary>
        public byte[] buffer;

        public BufferedImage(int width, int height, byte[] buffer)
        {
            this.width = width;
            this.height = height;
            this.buffer = buffer;
        }

        /// <summary>
        /// Получить байт значения альфа цвета пиксела
        /// </summary>
        public byte GetPixelAlpha(int x, int y) => buffer[y * height * 4 + x * 4 + 3];

        /// <summary>
        /// Получить цвет пикселя
        /// </summary>
        //public Vec4 GetPixel(int x, int y)
        //{
        //    int index = y * height * 4 + x * 4;
        //    byte r = buffer[index];
        //    byte g = buffer[index + 1];
        //    byte b = buffer[index + 2];
        //    byte a = buffer[index + 3];
        //    return new Vec4(Bf(r), Bf(g), Bf(b), Bf(a));
        //}

        //private float Bf(byte c) => c / 255f;
    }
}
