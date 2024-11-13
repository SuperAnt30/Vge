namespace WinGL.Util
{
    /// <summary>
    /// Объект изображения
    /// </summary>
    public class BufferedImage
    {
        /// <summary>
        /// Ширина
        /// </summary>
        public readonly int Width;
        /// <summary>
        /// Высотп
        /// </summary>
        public readonly int Height;
        /// <summary>
        /// Массив байт
        /// </summary>
        public readonly byte[] Buffer;
        /// <summary>
        /// Нужно ли делать MinMap
        /// </summary>
        public bool FlagMipMap;
        /// <summary>
        /// Номер активации текустуры
        /// </summary>
        public uint ActiveTextureIndex = 0;

        public BufferedImage(int width, int height, byte[] buffer)
        {
            Width = width;
            Height = height;
            Buffer = buffer;
        }

        /// <summary>
        /// Получить байт значения альфа цвета пиксела
        /// </summary>
        public byte GetPixelAlpha(int x, int y) => Buffer[y * Height * 4 + x * 4 + 3];

        /// <summary>
        /// Получить цвет пикселя
        /// </summary>
        //public 4 GetPixel(int x, int y)
        //{
        //    int index = y * height * 4 + x * 4;
        //    byte r = buffer[index];
        //    byte g = buffer[index + 1];
        //    byte b = buffer[index + 2];
        //    byte a = buffer[index + 3];
        //    return new 4(Bf(r), Bf(g), Bf(b), Bf(a));
        //}

        //private float Bf(byte c) => c / 255f;
    }
}
