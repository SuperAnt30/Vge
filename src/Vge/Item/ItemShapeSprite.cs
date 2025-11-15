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
        public VertexEntityBuffer GenBuffer()
        {
            QuadSide[] quads = new QuadSide[2];
            int min = -8;
            int max = 8;
            // Закоменченно из-за разности предмета спрайта 32 к блокам и моделям 16. Ибо в 2 раза больше был. 15.11.2025
            //if (_res.CountWidth > 1)
            //{
            //    min *= _res.CountWidth;
            //    max *= _res.CountWidth;
            //}
            quads[0] = new QuadSide();
            quads[0].SetSide(Pole.Up, false, min, 0, min, max, 2, max).SetTexture(_res.Index, 
                0, 0, 16 * _res.CountWidth, 16 * _res.CountHeight);
            quads[1] = new QuadSide();
            quads[1].SetSide(Pole.Down, false, min, 2, min, max, 2, max).SetTexture(_res.Index, 
                0, 0, 16 * _res.CountWidth, 16 * _res.CountHeight);

            return Convert(quads);
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
