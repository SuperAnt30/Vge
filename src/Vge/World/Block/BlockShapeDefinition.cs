using System;
using System.Runtime.CompilerServices;
using Vge.Item;
using Vge.Json;
using Vge.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Объект отвечает за определяение формы блока
    /// </summary>
    public class BlockShapeDefinition : ItemShapeDefinition
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

        /// <summary>
        /// Объект результата квада
        /// </summary>
        private QuadSide[][] _quads;
        /// <summary>
        /// Индекс варианта
        /// </summary>
        private int _indexV;
        /// <summary>
        /// Контраст
        /// </summary>
        private bool _sharpness;
        /// <summary>
        /// Ветер
        /// </summary>
        private int _wind;

        public BlockShapeDefinition(string alias) : base(alias) { }

        /// <summary>
        /// Запустить определение жидкой формы
        /// </summary>
        public SideLiquid[] RunShapeLiquidFromJson(JsonCompound state, JsonCompound shapes)
        {
            _log = Ctb.Liquid;
            try
            {
                if (state.IsKey(Ctb.Liquid))
                {
                    // Имеется форма
                    _log = Ctb.Shape;
                    JsonCompound shape = shapes.GetObject(state.GetString(Ctb.Liquid));
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
                throw new Exception(Sr.GetString(Sr.ErrorReadJsonBlockShape, _alias, _log), ex);
            }

            return new SideLiquid[0];
        }

        /// <summary>
        /// Запуск определения формы для блока
        /// </summary>
        public QuadSide[][] RunShapeBlockFromJson(JsonCompound state, JsonCompound shapes)
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

                    foreach (JsonCompound view in variants)
                    {
                        _log = Ctb.Shape;
                        _ShapeBlock(view, shapes);
                        _indexV++;
                    }

                    _CullFaceAll();
                    _ForceDrawFace();
                    return _quads;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(Sr.GetString(Sr.ErrorReadJsonBlockShape, _alias, _log), ex);
            }

            return new QuadSide[][] { new QuadSide[] { new QuadSide() } };
        }

        private void _ShapeBlock(JsonCompound view, JsonCompound shapes)
        {
            string nameShape = view.GetString(Ctb.Shape);
            if (nameShape == "") return;

            // Собираем дополнительные данные на фигуру
            _shapeAdd.RunShape(view, true, 1);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _Elements(JsonCompound element)
        {
            // Контраст
            _sharpness = element.IsKey(Ctb.Sharpness);
            // Ветер
            _wind = element.GetInt(Ctb.Wind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _ElementAdd(ShapeFace shapeFace)
        {
            shapeFace.Add(_sharpness, _wind);
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
                    SpriteData resTexture = _shapeTexture.GetResult(face.GetString(Ctb.TextureFace));
                    sideLiquid = new SideLiquid(side, shade, resTexture.Index, typeColor);
                    if (resTexture.CountHeight > 1)
                    {
                        byte frame = (byte)resTexture.CountHeight;
                        // Текучая жидкость, ромбом захыватывает 4 спрайта, по этому надо один ряд запасной
                        if (side > 1) frame--;
                        sideLiquid.SetAnimal(frame, (byte)face.GetInt(Ctb.Pause));
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
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonNotFacesShape, _alias));
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
