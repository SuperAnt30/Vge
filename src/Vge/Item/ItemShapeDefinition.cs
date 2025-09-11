using System;
using System.Runtime.CompilerServices;
using Vge.Json;
using Vge.World.Block;

namespace Vge.Item
{
    /// <summary>
    /// Объект отвечает за определяение формы предмета, аналог блока но без тонкостей соседей
    /// </summary>
    public class ItemShapeDefinition
    {
        /// <summary>
        /// Псевдоним
        /// </summary>
        protected readonly string _alias;
        /// <summary>
        /// Объект данных готовой фигуры
        /// </summary>
        protected readonly ShapeAdd _shapeAdd = new ShapeAdd();
        /// <summary>
        /// Текстуры к фигуре
        /// </summary>
        protected readonly ShapeTexture _shapeTexture = new ShapeTexture();

        /// <summary>
        /// Для краша, название раздела
        /// </summary>
        protected string _log;
        /// <summary>
        /// Индекс квада
        /// </summary>
        protected int _indexQ;

        /// <summary>
        /// Объект результата квада
        /// </summary>
        private QuadSide[] _quads;

        public ItemShapeDefinition(string alias) => _alias = alias;

        /// <summary>
        /// Запуск определения формы для предмета, где view это объект доп параметров модели
        /// </summary>
        public QuadSide[] RunShapeItemFromJson(JsonCompound view, JsonCompound shape, int sizeSprite)
        {
            _log = Cti.Shape;
            try
            {
                // Собираем дополнительные данные на фигуру
                _shapeAdd.RunShape(view, false, sizeSprite);
                // Имеется форма
                _log = Ctb.Texture;

                // Текстура
                _shapeTexture.RunShape(shape);

                _log = Ctb.Elements;
                JsonCompound[] elements = shape.GetArray(Ctb.Elements).ToArrayObject();

                _log = "FacesCount";
                // Определяем количество квадов
                int i;
                _indexQ = 0;
                for (i = 0; i < elements.Length; i++)
                {
                    _indexQ += elements[i].GetArray(Ctb.Faces).GetCount();
                }
                _quads = new QuadSide[_indexQ];
                _indexQ = 0;
                // Заполняем квады
                for (i = 0; i < elements.Length; i++)
                {
                    _Element(elements[i]);
                }
                return _quads;
            }
            catch (Exception ex)
            {
                throw new Exception(Sr.GetString(Sr.ErrorReadJsonItemShape, _alias, _log), ex);
            }
        }

        protected void _Element(JsonCompound element)
        {
            JsonCompound[] faces;
            ShapeFace shapeFace = new ShapeFace(_shapeAdd, _shapeTexture);
            float[] ar;

            // Определяем размер
            _log = Ctb.From;
            ar = element.GetArray(Ctb.From).ToArrayFloat();
            shapeFace.SetFrom(ar[0], ar[1], ar[2]);
            _log = Ctb.To;
            ar = element.GetArray(Ctb.To).ToArrayFloat();
            shapeFace.SetTo(ar[0], ar[1], ar[2]);

            // Вращение по центру блока
            _log = Ctb.Rotate;
            if (element.IsKey(Ctb.Rotate))
            {
                float[] rotate = element.GetArray(Ctb.Rotate).ToArrayFloat();
                float[] origin = element.IsKey(Ctb.Origin)
                    ? element.GetArray(Ctb.Origin).ToArrayFloat()
                    : new float[3];
                shapeFace.SetRotate(rotate[0], rotate[1], rotate[2],
                    origin[0], origin[1], origin[2]);

            }
            else
            {
                shapeFace.NotRotate();
            }

            // Отсутствие оттенка, т.е. зависит от стороны света, если true оттенка нет
            shapeFace.Shade = element.GetBool(Ctb.Shade);

            _Elements(element);

            // Собираем массив сторон
            _log = Ctb.Faces;
            faces = element.GetArray(Ctb.Faces).ToArrayObject();
            for (int i = 0; i < faces.Length; i++)
            {
                shapeFace.RunShape(faces[i]);
                _ElementAdd(shapeFace);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _Elements(JsonCompound element) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _ElementAdd(ShapeFace shapeFace)
            => _quads[_indexQ++] = shapeFace.GetQuadSide();
    }
}
