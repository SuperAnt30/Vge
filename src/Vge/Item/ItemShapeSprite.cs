using System;
using System.Collections.Generic;
using Vge.Entity.Render;
using Vge.Util;
using Vge.World;
using Vge.World.Block;

namespace Vge.Item
{
    /// <summary>
    /// Объект отвечает за создание формы от текстуры спрайта
    /// </summary>
    public class ItemShapeSprite
    {
        /// <summary>
        /// Псевдоним
        /// </summary>
        private readonly string _alias;
        /// <summary>
        /// Индекс расположения текстуры в атласе и количество кадров
        /// </summary>
        private readonly SpriteData _res;

        public ItemShapeSprite(string alias, string pathTexture)
        {
            _alias = alias;
            _res = BlocksReg.BlockItemAtlas.AddSprite(pathTexture);
            if (_res.Index == -1)
            {
                // Ошибка текстуры [] предмета []
                throw new Exception(Sr.GetString(Sr.ErrorTextureItem, _alias, pathTexture));
            }
        }

        /// <summary>
        /// Сгенерировать буфер
        /// </summary>
        public VertexEntityBuffer GenBufferOld()
        {
            QuadSide[] quads = new QuadSide[2];
            int min = -8;
            int max = 8;
            int h = 2;
            // Закоменченно из-за разности предмета спрайта 32 к блокам и моделям 16. Ибо в 2 раза больше был. 15.11.2025
            //if (_res.CountWidth > 1)
            //{
            //    min *= _res.CountWidth;
            //    max *= _res.CountWidth;
            //}
            quads[0] = new QuadSide();
            quads[0].SetSide(Pole.Up, false, min, h, min, max, h, max).SetTexture(_res.Index, 
                0, 0, 16 * _res.CountWidth, 16 * _res.CountHeight);

            quads[1] = new QuadSide();
            quads[1].SetSide(Pole.Down, false, min, 0, min, max, 0, max).SetTexture(_res.Index, 
                0, 16 * _res.CountHeight, 16 * _res.CountWidth, 0);

            return Convert(quads);
        }

        /// <summary>
        /// Сгенерировать буфер
        /// </summary>
        public VertexEntityBuffer GenBuffer()
        {
            int size = Ce.TextureSpriteBlockSize;
            int width = _res.CountWidth * size; 
            int heigth = _res.CountHeight * size;

            QuadSide quad;
            List<QuadSide> quads = new List<QuadSide>();
            int min = -8;
            int max = 8;
            float xf, yf;
            int h = 1;

            // Из-за разности предмета спрайта 32 к блокам и моделям 16.
            int cw = _res.CountWidth / 2;
            if (cw > 1)
            {
                min *= cw;
                max *= cw;
            }

            byte[] buffer = BlocksReg.BlockItemAtlas.GetSprite(_res);
            byte b;
            for (int y = 0; y < heigth; y++)
            {
                yf = min + y * .5f;
                for (int x = 0; x < width; x++)
                {
                    xf = min + x * .5f;
                    b = buffer[(y * heigth + x) * 4 + 3];
                    if (b > 250)
                    {
                        if (y == 0 || buffer[((y - 1) * heigth + x) * 4 + 3] < 249)
                        {
                            // Топ
                            quad = new QuadSide();
                            quad.SetSide(Pole.North, false, xf, 0, yf, xf + .5f, h, yf + .5f)
                                .SetTexture(_res.Index, x, y, x + 1, y + 1);
                            quads.Add(quad);
                        }
                        if (y == heigth - 1 || buffer[((y + 1) * heigth + x) * 4 + 3] < 249)
                        {
                            // Низ
                            quad = new QuadSide();
                            quad.SetSide(Pole.South, false, xf, 0, yf, xf + .5f, h, yf + .5f)
                                .SetTexture(_res.Index, x, y, x + 1, y + 1);
                            quads.Add(quad);
                        }
                        if (x == 0 || buffer[(y * heigth + x - 1) * 4 + 3] < 249)
                        {
                            // Лево
                            quad = new QuadSide();
                            quad.SetSide(Pole.West, false, xf, 0, yf, xf + .5f, h, yf + .5f)
                                .SetTexture(_res.Index, x, y, x + 1, y + 1);
                            quads.Add(quad);
                        }
                        if (x == width - 1 || buffer[(y * heigth + x + 1) * 4 + 3] < 249)
                        {
                            // Лево
                            quad = new QuadSide();
                            quad.SetSide(Pole.East, false, xf, 0, yf, xf + .5f, h, yf + .5f)
                                .SetTexture(_res.Index, x, y, x + 1, y + 1);
                            quads.Add(quad);
                        }
                    }
                }
            }

            // Вверх
            quad = new QuadSide();
            quad.SetSide(Pole.Up, false, min, h, min, max, h, max).SetTexture(_res.Index,
                0, 0, width, heigth);
            quads.Add(quad);

            // Низ
            quad = new QuadSide();
            quad.SetSide(Pole.Down, false, min, 0, min, max, 0, max).SetTexture(_res.Index,
                0, heigth, width, 0);
            quads.Add(quad);

            // Количество квадов на модели в мире
            //Console.WriteLine(_alias + " " + quads.Count);

            return Convert(quads.ToArray());
        }

        /// <summary>
        /// Сгенерировать буфер для GUI
        /// </summary>
        public VertexEntityBuffer GenBufferGui(int sizeSprite)
        {
            QuadSide[] quads = new QuadSide[1];
            int size = 8 * sizeSprite;
            if (_res.CountWidth > 1)
            {
                size *= _res.CountWidth;
            }
            quads[0] = new QuadSide();
            quads[0].SetSide(Pole.North, false, -size, -size, 0, size, size, 0).SetTexture(_res.Index,
                0, 0, 16 * _res.CountWidth, 16 * _res.CountHeight, 180);

            return ConvertGui(quads);
        }

        /// <summary>
        /// Конвертировать квады в сетку сущности
        /// </summary>
        /// <param name="quads">Массив квадов</param>
        public static VertexEntityBuffer Convert(QuadSide[] quads)
        {
            // Генерируем буффер
            List<float> listFloat = new List<float>();
            List<int> listInt = new List<int>();
            foreach (QuadSide quad in quads)
            {
                quad.GenBuffer(listFloat, listInt);
            }
            return new VertexEntityBuffer(listFloat.ToArray(), listInt.ToArray());
        }

        /// <summary>
        /// Конвертировать квады в сетку сущности для Gui
        /// </summary>
        /// <param name="quads">Массив квадов</param>
        public static VertexEntityBuffer ConvertGui(QuadSide[] quads)
        {
            // Генерируем буффер
            List<float> listFloat = new List<float>();
            List<int> listInt = new List<int>();
            foreach (QuadSide quad in quads)
            {
                quad.GenBuffer(listFloat, listInt);
            }
            return new VertexEntityBuffer(listFloat.ToArray(), listInt.ToArray());
        }

    }
}
