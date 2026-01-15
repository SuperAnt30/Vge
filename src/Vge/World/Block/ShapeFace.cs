using Vge.Json;
using Vge.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Сторона у фигуры
    /// </summary>
    public class ShapeFace
    {
        /// <summary>
        /// Отсутствие оттенка, т.е. зависит от стороны света, если true оттенка нет
        /// </summary>
        public bool Shade;

        /// <summary>
        /// Дополнительные параметры
        /// </summary>
        private readonly ShapeAdd _shapeAdd;
        /// <summary>
        /// Текстура фигуры
        /// </summary>
        private readonly ShapeTexture _shapeTexture;
        /// <summary>
        /// Объект формировании стороны
        /// </summary>
        private QuadSide _quad;
        /// <summary>
        /// Размер формы
        /// </summary>
        private float _x1, _y1, _z1, _x2, _y2, _z2;
        /// <summary>
        /// Имеется ли вращение
        /// </summary>
        private bool _isRotate;
        /// <summary>
        /// Параметры вращения
        /// </summary>
        private float _xR, _yR, _zR;
        /// <summary>
        /// Координаты центра вращения
        /// </summary>
        private float _xO, _yO, _zO;

        public ShapeFace(ShapeAdd shapeAdd, ShapeTexture shapeTexture)
        {
            _shapeAdd = shapeAdd;
            _shapeTexture = shapeTexture;
        }
        /// <summary>
        /// Указываем начальную точку параллелепипеда
        /// </summary>
        public void SetFrom(float x, float y, float z)
        {
            _x1 = x;
            _y1 = y;
            _z1 = z;
        }
        /// <summary>
        /// Указываем конечную точку параллелепипеда
        /// </summary>
        public void SetTo(float x, float y, float z)
        {
            _x2 = x;
            _y2 = y;
            _z2 = z;
        }
        /// <summary>
        /// Задаём вращение блока от центра, в градусах
        /// </summary>
        public void SetRotate(float xR, float yR, float zR, float xO, float yO, float zO)
        {
            _isRotate = true;
            _xR = xR;
            _yR = yR;
            _zR = zR;
            _xO = xO / 16f;
            _yO = yO / 16f;
            _zO = zO / 16f;
        }
        /// <summary>
        /// Параметр нет вращения
        /// </summary>
        public void NotRotate()
        {
            _isRotate = false;
            _yR = _xR = _zR = 0;
        }

        public void RunShape(JsonCompound face)
        {
            _quad = new QuadSide((byte)face.GetInt(Ctb.TypeColor));

            Pole pole = PoleConvert.GetPole(face.GetString(Ctb.Side));

            if (_shapeAdd.IsScale)
            {
                float scale = _shapeAdd.Scale;
                _quad.SetSide(pole, Shade, 
                    _x1 * scale, _y1 * scale, _z1 * scale,
                    _x2 * scale, _y2 * scale, _z2 * scale);
                if (_isRotate)
                {
                    _quad.SetRotate(_xR, _yR, _zR, _xO * scale, _yO * scale, _zO * scale);
                }
            }
            else
            {
                _quad.SetSide(pole, Shade, _x1, _y1, _z1, _x2, _y2, _z2);
                if (_isRotate)
                {
                    _quad.SetRotate(_xR, _yR, _zR, _xO, _yO, _zO);
                }
            }
            
            if (_shapeAdd.IsRotation)
            {
               _quad.SetRotateAdd(_shapeAdd.Rotate[0], _shapeAdd.Rotate[1], _shapeAdd.Rotate[2], _shapeAdd.Scale);
            }
            if (_shapeAdd.IsOffset)
            {
                _quad.SetTranslate(_shapeAdd.Offset[0], _shapeAdd.Offset[1], _shapeAdd.Offset[2]);
            }

            // Размеры текстуры
            float u1, v1, u2, v2;

            if (face.IsKey(Ctb.Uv))
            {
                float[] ar = face.GetArray(Ctb.Uv).ToArrayFloat();
                u1 = ar[0];
                v1 = ar[1];
                u2 = ar[2];
                v2 = ar[3];
            }
            else
            {
                u1 = v1 = 0;
                u2 = v2 = 16;
            }
            // Вращение текстуры 0 || 90 || 180 || 270
            int uvRotate = face.GetInt(Ctb.UvRotate);

            SpriteData resTexture = _shapeTexture.GetResult(face.GetString(Ctb.TextureFace));
            _quad.SetTexture(resTexture.Index, u1, v1, u2, v2, uvRotate);

            if (_shapeAdd.IsRotateX90)
            {
                _quad.SetRotateX90(Shade);
            }
            if (_shapeAdd.RotateY != 0)
            {
                _quad.SetRotateY(_shapeAdd.RotateY, Shade);
            }

            if (resTexture.CountHeight > 1)
            {
                _quad.SetAnimal((byte)resTexture.CountHeight, (byte)face.GetInt(Ctb.Pause));
            }
        }

        public void Add(bool sharpness, int wind)
        {
            // Резкость
            if (sharpness)
            {
                _quad.SetSharpness();
            }
            // Ветер
            if (wind != 0)
            {
                _quad.SetWind((byte)wind);
            }
        }

        /// <summary>
        /// С какой стороны
        /// </summary>
        public int Side => _quad.Side;

        /// <summary>
        /// Генерация макси в заданный массив, и возвращает true если принудительное рисование стороны
        /// </summary>
        public bool GenMask(ulong[] ar) => _quad.GenMask(ar);

        /// <summary>
        /// Не крайняя сторона, чтоб не отброкавать при прорисовке
        /// </summary>
        public void SetNotExtremeSide() => _quad.SetNotExtremeSide();

        /// <summary>
        /// Получить объект квада
        /// </summary>
        public QuadSide GetQuadSide()
        {
            // Нахождение нормали
            _quad.GenNormal();
            return _quad;
        }
    }
}
