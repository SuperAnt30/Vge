using System.Collections.Generic;
using WinGL.Util;

namespace Vge.Util
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
        public readonly bool FlagMipMap;
        /// <summary>
        /// Номер активации текустуры
        /// </summary>
        public readonly uint ActiveTextureIndex;

        /// <summary>
        /// Изображения для mipmap
        /// </summary>
        private BufferedImage[] _images;

        public BufferedImage(int width, int height, byte[] buffer,
            uint activeTextureIndex = 0, bool minmap = false)
        {
            Width = width;
            Height = height;
            Buffer = buffer;
            FlagMipMap = minmap;
            ActiveTextureIndex = activeTextureIndex;
            if (FlagMipMap)
            {
                _MipMap(Buffer, new List<BufferedImage>());
            }
        }

        public BufferedImage(byte[] buffer, int size)
        {
            Buffer = buffer;
            Width = Height = size;
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

        /// <summary>
        /// Создание уровней MipMap
        /// </summary>
        private void _MipMap(byte[] buffer, List<BufferedImage> list)
        {
            if (buffer.Length > Ce.TextureAtlasBlockCount * Ce.TextureAtlasBlockCount * 4)
            //if (buffer.Length > 16384) // 64 * 64
            {
                int w = (int)Mth.Sqrt(buffer.Length / 4);
                int w2 = w / 2;
                byte[] buf = new byte[w * w];
                int b, g, r, a, a1, c, x, y, x1, y1, index;

                for (x = 0; x < w; x += 2)
                {
                    for (y = 0; y < w; y += 2)
                    {
                        b = g = r = a = c = 0;
                        for (x1 = 0; x1 < 2; x1++)
                        {
                            for (y1 = 0; y1 < 2; y1++)
                            {
                                index = ((y + y1) * w + x + x1) * 4;
                                a1 = buffer[index + 3];
                                a += a1;
                                if (a1 > 0)
                                {
                                    b += buffer[index + 0];
                                    g += buffer[index + 1];
                                    r += buffer[index + 2];
                                    //a += a1;
                                    c++;
                                }
                            }
                        }
                        index = ((y / 2) * w2 + x / 2) * 4;
                        if (c > 1)
                        {
                            buf[index + 0] = (byte)(b / c);
                            buf[index + 1] = (byte)(g / c);
                            buf[index + 2] = (byte)(r / c);
                            buf[index + 3] = (byte)(a / c);
                        }
                        //buf[index + 3] = (byte)(a / 4);
                    }
                }
                list.Add(new BufferedImage(buf, w2));
                _MipMap(buf, list);
            }
            else
            {
                _images = list.ToArray();
            }
        }

        /// <summary>
        /// Количество уровней MipMap
        /// </summary>
        public int CountMipMap() => _images != null ? _images.Length : 0;

        /// <summary>
        /// Получить буфер уровня MipMap
        /// </summary>
        public BufferedImage GetLevelMipMap(int index) => _images[index];

    }
}
