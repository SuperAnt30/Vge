using System;
using Vge.Json;
using Vge.Util;
using WinGL.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Объект отвечает за определяение формы блока
    /// </summary>
    public class BlockShapeDefinition
    {
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

        private readonly BlockBase _block;
        /// <summary>
        /// Объект данных готовой фигуры
        /// </summary>
        private readonly ShapeAdd _shapeAdd = new ShapeAdd();
        /// <summary>
        /// Текстуры к фигуре
        /// </summary>
        private readonly ShapeTexture _shapeTexture = new ShapeTexture();

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

        public BlockShapeDefinition(BlockBase block) => _block = block;

        /// <summary>
        /// Запустить определение жидкой формы
        /// </summary>
        public SideLiquid[] RunShapeLiquidFromJson(JsonCompound state, JsonCompound shapes)
        {
            _log = Ctb.Variant;
            try
            {
                if (state.IsKey(Ctb.Variant))
                {
                    // Имеется форма
                    _log = Ctb.Shape;
                    JsonCompound shape = shapes.GetObject(state.GetString(Ctb.Variant));
                    // Текстура
                    _log = Ctb.Texture;
                    _shapeTexture.RunShape(shape);

                    _log = Ctb.Element;
                    if (shape.IsKey(Ctb.Element))
                    {
                        return _ElementLiquid(shape.GetObject(Ctb.Element));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(Sr.GetString(Sr.ErrorReadJsonBlockShape, _block.Alias, _log), ex);
            }

            return new SideLiquid[0];
        }

        /// <summary>
        /// Запуск определения формы
        /// </summary>
        public QuadSide[][] RunShapeFromJson(JsonCompound state, JsonCompound shapes)
        {
            _log = Ctb.Variants;
            try
            {
                JsonCompound[] variants = state.GetArray(Ctb.Variants).ToArrayObject();
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
                        _log = Ctb.Shape;
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
            string nameShape = variant.GetString(Ctb.Shape);
            if (nameShape == "") return;

            // Собираем дополнительные данные на фигуру
            _shapeAdd.RunShape(variant);
            // Имеется форма
            JsonCompound shape = shapes.GetObject(nameShape);

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
            _log = Ctb.From;
            arInt = element.GetArray(Ctb.From).ToArrayInt();
            shapeFace.SetFrom(arInt[0], arInt[1], arInt[2]);
            _log = Ctb.To;
            arInt = element.GetArray(Ctb.To).ToArrayInt();
            shapeFace.SetTo(arInt[0], arInt[1], arInt[2]);

            // Перемещение элемента
            _log = Ctb.Translate;
            if (element.IsKey(Ctb.Translate))
            {
                float[] xyz = element.GetArray(Ctb.Translate).ToArrayFloat();
                shapeFace.SetTranslate(xyz[0], xyz[1], xyz[2]);
            }
            else
            {
                shapeFace.NotTranslate();
            }

            // Вращение по центру блока
            _log = Ctb.Rotate;
            if (element.IsKey(Ctb.Rotate))
            {
                float[] ypr = element.GetArray(Ctb.Rotate).ToArrayFloat();
                shapeFace.SetRotate(ypr[0], ypr[1], ypr[2]);
            }
            else
            {
                shapeFace.NotRotate();
            }

            // Отсутствие оттенка, т.е. зависит от стороны света, если true оттенка нет
            shapeFace.Shade = element.GetBool(Ctb.Shade);
            // Контраст
            bool sharpness = element.IsKey(Ctb.Sharpness);
            // Ветер
            int wind = element.GetInt(Ctb.Wind);
            
            // Собираем массив сторон
            _log = Ctb.Faces;
            faces = element.GetArray(Ctb.Faces).ToArrayObject();
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

        private SideLiquid[] _ElementLiquid(JsonCompound element)
        {
            SideLiquid[] sideLiquids = new SideLiquid[6];
            SideLiquid sideLiquid;
            JsonCompound[] faces;
            JsonCompound face;

            // Отсутствие оттенка, т.е. зависит от стороны света, если true оттенка нет
            bool shade = element.GetBool(Ctb.Shade);
            // Ветер
            int wind = element.GetInt(Ctb.Wind);
            // Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода, 4 - свой цвет.
            byte typeColor = (byte)element.GetInt(Ctb.TypeColor);

            // Собираем массив сторон
            _log = Ctb.Faces;
            faces = element.GetArray(Ctb.Faces).ToArrayObject();
            for (int i = 0; i < faces.Length; i++)
            {
                face = faces[i];
                Pole pole = PoleConvert.GetPole(face.GetString(Ctb.Side));
                if (pole != Pole.All)
                {
                    int side = (int)pole;
                    Vector2i resTexture = _shapeTexture.GetResult(face.GetString(Ctb.TextureFace));
                    sideLiquid = new SideLiquid(side, shade, resTexture.X, typeColor);
                    if (resTexture.Y > 1)
                    {
                        sideLiquid.SetAnimal((byte)resTexture.Y, (byte)face.GetInt(Ctb.Pause));
                    }
                    // Ветер
                    if (wind != 0)
                    {
                        sideLiquid.SetWind((byte)wind);
                    }
                    sideLiquids[side] = sideLiquid;
                }
            }
            // Проверяем наличие всех сторон
            for (int i = 0; i < 6; i++)
            {
                if (sideLiquids[i] == null)
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonNotFacesShape, _block.Alias));
                }
            }
            
            return sideLiquids;
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
                        if (MaskCullFaces[i][k][j] != ulong.MaxValue)
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
