using System;
using Vge.Json;

namespace Vge.World.Block
{
    /// <summary>
    /// Объект отвечает за определяение формы блока
    /// </summary>
    public class BlockShapeDefinition
    {
        private readonly BlockBase _block;

        /// <summary>
        /// Маска на все варианты и стороны, 4 ulong-a (256 бит)
        /// </summary>
        public ulong[][][] MaskCullFaces { get; private set; }
        /// <summary>
        /// Для оптимизации отбраковка стороны, чтоб не использовать маску
        /// </summary>
        public bool[][] CullFaces { get; private set; }
        /// <summary>
        /// Принудительное рисование стороны, true если сторона не касается края
        /// </summary>
        public bool[][] ForceDrawFaces { get; private set; }
        /// <summary>
        /// Принудительное рисование не крайней стороны 
        /// </summary>
        public bool[][] ForceDrawNotExtremeFaces { get; private set; }

        /// <summary>
        /// Отбраковка всех сторон всех сторон во всех вариантах
        /// </summary>
        public bool CullFaceAll = false;
        /// <summary>
        /// Принудительное рисование всех сторон
        /// </summary>
        public bool ForceDrawFace = true;

        /// <summary>
        /// Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода, 4 - свой цвет
        /// </summary>
        public byte BiomeColor = 0;

        public BlockShapeDefinition(BlockBase block) => _block = block;

        /// <summary>
        /// Для краша, название раздела
        /// </summary>
        private string _log;
        /// <summary>
        /// Объект результата квада
        /// </summary>
        private QuadSide[][] _quads;
        /// <summary>
        /// Индекс варианта
        /// </summary>
        private int _indexV;
        /// <summary>
        /// Индекс квада
        /// </summary>
        private int _indexQ;

        /// <summary>
        /// Объект данных готовой фигуры
        /// </summary>
        private readonly ShapeAdd _shapeAdd = new ShapeAdd();
        /// <summary>
        /// Текстуры к фигуре
        /// </summary>
        private readonly ShapeTexture _shapeTexture = new ShapeTexture();

        /// <summary>
        /// Запуск определения формы
        /// </summary>
        public QuadSide[][] RunShapeFromJson(JsonCompound state, JsonCompound shapes)
        {
            _log = "Variants";
            try
            {
                JsonCompound[] variants = state.GetArray("Variants").ToArrayObject();
                if (variants.Length > 0)
                {
                    _quads = new QuadSide[variants.Length][];
                    MaskCullFaces = new ulong[variants.Length][][];
                    ForceDrawFaces = new bool[variants.Length][];
                    ForceDrawNotExtremeFaces = new bool[variants.Length][];
                    CullFaces = new bool[variants.Length][];

                    _indexV = 0;

                    foreach (JsonCompound variant in variants)
                    {
                        _log = "Shape";
                        _Shape(variant, shapes);
                        _indexV++;
                    }

                    _CullFaceAll();
                    _ForceDrawFace();
                    return _quads;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(Sr.GetString(Sr.ErrorReadJsonBlockShape, _block.Alias, _log), ex);
            }

            return new QuadSide[][] { new QuadSide[] { new QuadSide() } };
        }

        private void _Shape(JsonCompound variant, JsonCompound shapes)
        {
            string nameShape = variant.GetString("Shape");
            if (nameShape == "") return;

            // Собираем дополнительные данные на фигуру
            _shapeAdd.RunShape(variant);
            // Имеется форма
            JsonCompound shape = shapes.GetObject(nameShape);

            _log = "Texture";
            // Текстура
            _shapeTexture.RunShape(shape);

            _log = "Elements";
            JsonCompound[] elements = shape.GetArray("Elements").ToArrayObject();

            _log = "FacesCount";
            // Определяем количество квадов
            int i;
            _indexQ = 0;
            for (i = 0; i < elements.Length; i++)
            {
                _indexQ += elements[i].GetArray("Faces").GetCount();
            }
            _quads[_indexV] = new QuadSide[_indexQ];
            MaskCullFaces[_indexV] = new ulong[6][];
            ForceDrawFaces[_indexV] = new bool[] { true, true, true, true, true, true };
            ForceDrawNotExtremeFaces[_indexV] = new bool[6];
            CullFaces[_indexV] = new bool[6];

            _indexQ = 0;
            // Заполняем квады
            for (i = 0; i < elements.Length; i++)
            {
                _Element(elements[i]);
            }
        }

        private void _Element(JsonCompound element)
        {
            JsonCompound[] faces;
            ShapeFace shapeFace = new ShapeFace(_shapeAdd, _shapeTexture);
            int[] arInt;

            // Определяем размер
            _log = "FromTo";
            arInt = element.GetArray("From").ToArrayInt();
            shapeFace.SetFrom(arInt[0], arInt[1], arInt[2]);

            arInt = element.GetArray("To").ToArrayInt();
            shapeFace.SetTo(arInt[0], arInt[1], arInt[2]);

            // Вращение
            _log = "Rotate";
            if (element.IsKey("Rotate"))
            {
                float[] ypr = element.GetArray("Rotate").ToArrayFloat();
                shapeFace.SetRotate(ypr[0], ypr[1], ypr[2]);
            }
            else
            {
                shapeFace.NotRotate();
            }

            // Отсутствие оттенка, т.е. зависит от стороны света, если true оттенка нет
            shapeFace.Shade = element.GetBool("Shade");
            // Контраст
            bool sharpness = element.IsKey("Sharpness");
            // Ветер
            int wind = element.GetInt("Wind");
            
            // Собираем массив сторон
            _log = "Faces";
            faces = element.GetArray("Faces").ToArrayObject();
            for (int i = 0; i < faces.Length; i++)
            {
                shapeFace.RunShape(faces[i]);
                shapeFace.Add(sharpness, wind);

                if (MaskCullFaces[_indexV][shapeFace.Side] == null)
                {
                    MaskCullFaces[_indexV][shapeFace.Side] = new ulong[4];
                }

                if (shapeFace.GenMask(MaskCullFaces[_indexV][shapeFace.Side]))
                {
                    ForceDrawNotExtremeFaces[_indexV][shapeFace.Side] = true;
                    shapeFace.SetNotExtremeSide();
                }

                _quads[_indexV][_indexQ++] = shapeFace.GetQuadSide();
            }
        }

        /// <summary>
        /// Определить быстрый статус на все стороны, отбраковка всех сторон
        /// </summary>
        private void _CullFaceAll()
        {
            CullFaceAll = true;
            for (int i = 0; i < MaskCullFaces.Length; i++)
            {
                for (int k = 0; k < 6; k++)
                {
                    if (MaskCullFaces[i][k] == null)
                    {
                        MaskCullFaces[i][k] = new ulong[4];
                    }
                    CullFaces[i][k] = true;
                    for (int j = 0; j < 4; j++)
                    {
                        if (MaskCullFaces[i][k][j] != 18446744073709551615L)
                        {
                            CullFaceAll = false;
                            CullFaces[i][k] = false;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Определить быстрый статус на все стороны, принудительное рисование всех сторон
        /// </summary>
        private void _ForceDrawFace()
        {
            ForceDrawFace = true;
            for (int i = 0; i < MaskCullFaces.Length; i++)
            {
                for (int k = 0; k < 6; k++)
                {
                    ForceDrawFaces[i][k] = true;
                    for (int j = 0; j < 4; j++)
                    {
                        if (MaskCullFaces[i][k][j] != 0)
                        {
                            ForceDrawFace = false;
                            ForceDrawFaces[i][k] = false;
                            break;
                        }
                    }
                }
            }
        }
    }
}
