using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Vge.Util;
using WinGL.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Создание атласа блоков
    /// </summary>
    public class GeneratingBlockAtlas
    {
        /// <summary>
        /// Высота
        /// </summary>
        private int _height;
        /// <summary>
        /// Шаг строк ширина * 4
        /// </summary>
        private int _stride;
        /// <summary>
        /// Графический буфер
        /// </summary>
        private byte[] _buffer;
        /// <summary>
        /// Маска для понимания где уже имеется спрайт
        /// </summary>
        private bool[,] _mask; 

        /// <summary>
        /// Справочник текстур, название текстуры, индекс расположения текстуры в атласе и количество кадров
        /// </summary>
        private readonly Dictionary<string, Vector2i> _textures = new Dictionary<string, Vector2i>();

        /// <summary>
        /// Размер текстуры блока
        /// </summary>
        private int _textureBlockSize = 16;
        /// <summary>
        /// Количество спрайтов на стороне текстурного атласа
        /// </summary>
        private int _textureAtlasBlockCount = 64;

        //private readonly Logger _logger = new Logger("Debug");
        //private readonly Profiler _profiler;

        //public GeneratingBlockAtlas() => _profiler = new Profiler(_logger);

        /// <summary>
        /// Создать картинку для атласа
        /// </summary>
        /// <param name="textureAtlasBlockCount">Количество спрайтов на стороне текстурного атласа</param>
        /// <param name="textureBlockSize">Размер спрайта в px</param>
        public void CreateImage(int textureAtlasBlockCount, int textureBlockSize)
        {
            Ce.TextureAtlasBlockCount = _textureAtlasBlockCount = textureAtlasBlockCount;
            Ce.ShaderAnimOffset = 1f / Ce.TextureAtlasBlockCount;
            _textureBlockSize = textureBlockSize;
            _height = textureAtlasBlockCount * textureBlockSize;
            _stride = _height * 4;
            _buffer = new byte[_stride * _height];
            _mask = new bool[_textureAtlasBlockCount, _textureAtlasBlockCount];
        }

        public void EndImage(WindowMain window)
        {
            // Удаляем текстуры, так-как может быть иной размер
            window.Render.DeleteTextureAtlases();

            // Создаём заного текстуры
            BufferedImage bufferedImage = new BufferedImage(_height, _height, _buffer, 1, false);
            window.Render.AddTextureAtlasSharpness(bufferedImage);
            bufferedImage = new BufferedImage(_height, _height, _buffer, 0, true);
            window.Render.AddTextureAtlasBlurry(bufferedImage);

#if DEBUG
            // Для отладки сохранить в файл посмотреть как вышло

            // Отладка маски в txt
            string s = "";
            for (int y = 0; y < _textureAtlasBlockCount; y++)
            {
                s += "[";
                for (int x = 0; x < _textureAtlasBlockCount; x++)
                {
                    s += _mask[y, x] ? "*" : " ";
                }
                s += "]\r\n";
            }
            using (StreamWriter file = new StreamWriter("AtlasMask.txt"))
            {
                file.Write(s);
                file.Close();
            }

            // Отладка блочного атласа
            IntPtr ptr = Marshal.AllocHGlobal(_buffer.Length);
            Marshal.Copy(_buffer, 0, ptr, _buffer.Length);
            Bitmap bitmap = new Bitmap(_height, _height, _stride, PixelFormat.Format32bppArgb, ptr);
            bitmap.Save("atlas.png", ImageFormat.Png);
            Marshal.FreeHGlobal(ptr);

            // Отладка mipmap
            //BufferedImage buffered;
            //for (int i = 0; i < bufferedImage.CountMipMap(); i++)
            //{
            //    buffered = bufferedImage.GetLevelMipMap(i);
            //    ptr = Marshal.AllocHGlobal(buffered.Buffer.Length);
            //    Marshal.Copy(buffered.Buffer, 0, ptr, buffered.Buffer.Length);
            //    bitmap = new Bitmap(buffered.Width, buffered.Height, buffered.Width * 4, PixelFormat.Format32bppArgb, ptr);
            //    bitmap.Save("atlas-" + i + ".png", ImageFormat.Png);
            //    Marshal.FreeHGlobal(ptr);
            //}

#endif
            //_logger.Save();
            Clear();
        }

        /// <summary>
        /// Очистить буфер и справочник
        /// </summary>
        public void Clear()
        {
            _textures.Clear();
            _buffer = new byte[0];
        }

        public Vector2i AddSprite(string name)
        {
            if (_textures.ContainsKey(name))
            {
                // Такая текстура сгенерированна ранее, возвращаем просто ID
                return _textures[name];
            }

            string fileName = Options.PathTextures + name + ".png";
            if (File.Exists(fileName))
            {
                //_profiler.StartSection(name);
                int size = _textureBlockSize; // 16
                int sizeRGBA = size * 4; // 64
                BufferedImage buffered = BufferedFileImage.FileToBufferedImage(fileName);
                int index = -1;
                int w = 1;
                if (buffered.Width == size && buffered.Height >= size)
                {
                    index = buffered.Height > size
                        ? _GenIndexAnim(buffered.Height / size)
                        : _GenIndexOne();
                }
                else if(buffered.Width > size && buffered.Height > size)
                {
                    w = buffered.Width / size;
                    if (buffered.Width == w * size)
                    {
                        index = _GenIndexAnim2d(buffered.Height / size, w);
                    }
                }
                if (index != -1)
                {
                    Vector2i res = new Vector2i(index, _BufferCopy(index, w, buffered, name));
                    _textures.Add(name, res);
                    return res;
                }
                //_profiler.EndSectionLog();
            }
            return new Vector2i(-1, 0);
        }

        private int _BufferCopy(int index, int w, BufferedImage buffered, string name)
        {
            int size = _textureBlockSize; // 16
            int sizeRGBA = size * 4; // 64
            int sizeRGBAw = sizeRGBA * w;
            int index2 = index / _textureAtlasBlockCount * _stride * size
                + index % _textureAtlasBlockCount * sizeRGBA;
            for (int y = 0; y < buffered.Height; y++)
            {
                Buffer.BlockCopy(buffered.Buffer, y * sizeRGBAw,
                    _buffer, index2 + (_stride * y), sizeRGBAw);
            }
            return buffered.Height / _textureBlockSize;
        }

        /// <summary>
        /// Получить индекс для одного спрайта
        /// </summary>
        private int _GenIndexOne()
        {
             //for (int y = 0; y < _textureAtlasBlockCount; y++)
            for (int x = 0; x < _textureAtlasBlockCount; x++)
            {
                //for (int x = 0; x < _textureAtlasBlockCount; x++)
                for (int y = 0; y < _textureAtlasBlockCount; y++)
                {
                    if (!_mask[y, x])
                    {
                        _mask[y, x] = true;
                        return y * _textureAtlasBlockCount + x;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Получить индекс для анимационного спрайта, задаём количество спрайтов
        /// </summary>
        private int _GenIndexAnim(int lenght)
        {
            int count = _textureAtlasBlockCount - lenght + 1;
            for (int x = 0; x < _textureAtlasBlockCount; x++)
            {
                for (int y = 0; y < count; y++)
                {
                    if (!_mask[y, x])
                    {
                        if (_CheckHeight(x, y, lenght))
                        {
                            count = y + lenght;
                            for (int y2 = y; y2 < count; y2++)
                            {
                                _mask[y2, x] = true;
                            }
                            return y * _textureAtlasBlockCount + x;
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Получить индекс для анимационного спрайта, задаём количество спрайтов
        /// </summary>
        private int _GenIndexAnim2d(int lenght, int width)
        {
            //  lenght--;
            int count = _textureAtlasBlockCount - lenght + 1;
            for (int x = 0; x < _textureAtlasBlockCount; x++)
            {
                for (int y = 0; y < count; y++)
                {
                    if (!_mask[y, x])
                    {
                        if (_CheckHeight2d(x, y, lenght, width))
                        {
                            count = y + lenght;
                            int countW = x + width;
                            for (int y2 = y; y2 < count; y2++)
                            {
                                for (int x2 = x; x2 < countW; x2++)
                                {
                                    _mask[y2, x2] = true;
                                }
                            }
                            return y * _textureAtlasBlockCount + x;
                        }
                    }
                }
            }
            return -1;
        }

        private bool _CheckHeight(int x, int y, int lenght)
        {
            int count = y + lenght;
            for (int y2 = y; y2 < count; y2++)
            {
                if (_mask[y2, x]) return false;
            }
            return true;
        }


        private bool _CheckHeight2d(int x, int y, int lenght, int width)
        {
            int count = y + lenght;
            int countW = x + width;
            for (int y2 = y; y2 < count; y2++)
            {
                for (int x2 = x; x2 < countW; x2++)
                {
                    if (_mask[y2, x2]) return false;
                }
            }
            return true;
        }
    }
}
