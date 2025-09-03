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
            // TODO::2025-09-03 Вынести в константу, со всеми 16 для блоков и модели
            int size = 16;// BlocksReg.BlockItemAtlas.TextureBlockSize;
            QuadSide[] quads = new QuadSide[2];
            int min = -8;
            int max = 8;
            if (_res.CountWidth > 1)
            {
                min *= _res.CountWidth;
                max *= _res.CountWidth;
            }
            quads[0] = new QuadSide();
            quads[0].SetSide(Pole.Up, false, min, 0, min, max, 2, max).SetTexture(_res.Index, 0, 0, size * _res.CountWidth, size * _res.CountHeight);
            quads[1] = new QuadSide();
            quads[1].SetSide(Pole.Down, false, min, 2, min, max, 2, max).SetTexture(_res.Index, 0, 0, size * _res.CountWidth, size * _res.CountHeight);

            return Convert(quads);
        }

        /// <summary>
        /// Конвертировать квады в сетку сущности
        /// </summary>
        /// <param name="quads">Массив квадов</param>
        /// <param name="offsetX">Смещение по X</param>
        /// <param name="offsetZ">Смещение по Z</param>
        public static VertexEntityBuffer Convert(QuadSide[] quads, float offsetX = 0, float offsetZ = 0)
        {
            // Генерируем буффер
            List<float> listFloat = new List<float>();
            List<int> listInt = new List<int>();
            foreach (QuadSide quad in quads)
            {
                quad.GenBuffer(listFloat, listInt, offsetX, offsetZ);
            }
            return new VertexEntityBuffer(listFloat.ToArray(), listInt.ToArray());
        }

    }
}
