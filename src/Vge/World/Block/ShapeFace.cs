using Vge.Json;
using Vge.Util;
using WinGL.Util;

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
        private int _x1, _y1, _z1, _x2, _y2, _z2;
        /// <summary>
        /// Имеется ли смещение объекта после вращения
        /// </summary>
        private bool _isTranslate;
        /// <summary>
        /// Координаты смещения блока, задаются в пикселах 16 сторона блока
        /// </summary>
        private float _xT, _yT, _zT;
        /// <summary>
        /// Имеется ли вращение
        /// </summary>
        private bool _isRotate;
        /// <summary>
        /// Параметры вращения по центру блока
        /// </summary>
        private float _yaw, _pitch, _roll;

        public ShapeFace(ShapeAdd shapeAdd, ShapeTexture shapeTexture)
        {
            _shapeAdd = shapeAdd;
            _shapeTexture = shapeTexture;
        }
        /// <summary>
        /// Указываем начальную точку параллелепипеда
        /// </summary>
        public void SetFrom(int x, int y, int z)
        {
            _x1 = x;
            _y1 = y;
            _z1 = z;
        }
        /// <summary>
        /// Указываем конечную точку параллелепипеда
        /// </summary>
        public void SetTo(int x, int y, int z)
        {
            _x2 = x;
            _y2 = y;
            _z2 = z;
        }
        /// <summary>
        /// Задаём смещение в элементе, после вращения, в пикселах 16 на стороне блока
        /// </summary>
        public void SetTranslate(float x, float y, float z)
        {
            _isTranslate = true;
            _xT = x / 16f;
            _yT = y / 16f;
            _zT = z / 16f;
        }
        /// <summary>
        /// Параметр нет смещения
        /// </summary>
        public void NotTranslate()
        {
            _isTranslate = false;
            _xT = _yT = _zT = 0;
        }
        /// <summary>
        /// Задаём вращение блока от центра, в градусах
        /// </summary>
        public void SetRotate(float yaw, float pitch, float roll)
        {
            _isRotate = true;
            _yaw = yaw;
            _pitch = pitch;
            _roll = roll;
        }
        /// <summary>
        /// Параметр нет вращения
        /// </summary>
        public void NotRotate()
        {
            _isRotate = false;
            _yaw = _pitch = _roll = 0;
        }

        public void RunShape(JsonCompound face)
        {
            _quad = new QuadSide((byte)face.GetInt(Ctb.TypeColor));

            Pole pole = PoleConvert.GetPole(face.GetString(Ctb.Side));

            _quad.SetSide(pole, Shade, _x1, _y1, _z1, _x2, _y2, _z2);
            if (_isRotate)
            {
                _quad.SetRotate(_yaw, _pitch, _roll);
            }
            if (_isTranslate)
            {
                _quad.SetTranslate(_xT, _yT, _zT);
            }
            if (_shapeAdd.IsOffset)
            {
                _quad.SetTranslate(_shapeAdd.GetOffsetX(),
                    _shapeAdd.GetOffsetY(), _shapeAdd.GetOffsetZ());
            }

            // Размеры текстуры
            int u1, v1, u2, v2;

            if (face.IsKey(Ctb.Uv))
            {
                int[] arInt = face.GetArray(Ctb.Uv).ToArrayInt();
                u1 = arInt[0];
                v1 = arInt[1];
                u2 = arInt[2];
                v2 = arInt[3];
            }
            else
            {
                u1 = v1 = 0;
                u2 = v2 = 16;
            }
            // Вращение текстуры 0 || 90 || 180 || 270
            int uvRotate = face.GetInt(Ctb.UvRotate);

            if (_shapeAdd.RotateY != 0)
            {
                _quad.SetRotateY(_shapeAdd.RotateY, Shade);

                if (pole == Pole.Up && _shapeAdd.UvLock)
                {
                    uvRotate += _shapeAdd.RotateY;
                    if (uvRotate > 270) uvRotate -= 360;
                    if (_shapeAdd.RotateY == 90 || _shapeAdd.RotateY == 270)
                    {
                        int uv = v1;
                        v1 = u1;
                        u1 = uv;
                        uv = v2;
                        v2 = u2;
                        u2 = uv;
                    }
                }
            }

            Vector2i resTexture = _shapeTexture.GetResult(face.GetString(Ctb.TextureFace));
            _quad.SetTexture(resTexture.X, u1, v1, u2, v2, uvRotate);
            if (resTexture.Y > 1)
            {
                _quad.SetAnimal((byte)resTexture.Y, (byte)face.GetInt(Ctb.Pause));
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
