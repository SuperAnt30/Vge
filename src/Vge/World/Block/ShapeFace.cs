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
        /// Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода, 4 - свой цвет
        /// </summary>
        public byte BiomeColor = 0;
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
        /// Имеется ли вращение
        /// </summary>
        private bool _isRotate;
        /// <summary>
        /// Параметры вращения
        /// </summary>
        private float _yaw, _pitch, _roll;

        public ShapeFace(ShapeAdd shapeAdd, ShapeTexture shapeTexture)
        {
            _shapeAdd = shapeAdd;
            _shapeTexture = shapeTexture;
        }

        public void SetFrom(int x, int y, int z)
        {
            _x1 = x;
            _y1 = y;
            _z1 = z;
        }
        public void SetTo(int x, int y, int z)
        {
            _x2 = x;
            _y2 = y;
            _z2 = z;
        }
        public void SetRotate(float yaw, float pitch, float roll)
        {
            _isRotate = true;
            _yaw = yaw;
            _pitch = pitch;
            _roll = roll;
        }
        public void NotRotate()
        {
            _isRotate = false;
            _yaw = _pitch = _roll = 0;
        }

        public void RunShape(JsonCompound face)
        {
            byte biomeColor = (byte)face.GetInt("BiomeColor");
            if (biomeColor != 0)
            {
                BiomeColor = biomeColor;
            }
            _quad = new QuadSide(biomeColor);

            Pole pole = PoleConvert.GetPole(face.GetString("Side"));

            _quad.SetSide(pole, Shade, _x1, _y1, _z1, _x2, _y2, _z2);
            if (_isRotate)
            {
                _quad.SetRotate(_yaw, _pitch, _roll);
            }
            if (_shapeAdd.IsOffset)
            {
                _quad.SetTranslate(_shapeAdd.GetOffsetX(),
                    _shapeAdd.GetOffsetY(), _shapeAdd.GetOffsetZ());
            }

            // Размеры текстуры
            int u1, v1, u2, v2;

            if (face.IsKey("Uv"))
            {
                int[] arInt = face.GetArray("Uv").ToArrayInt();
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
            int uvRotate = face.GetInt("UvRotate");

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

            _quad.SetTexture(_shapeTexture.GetIndex(face.GetString("Texture")),
                u1, v1, u2, v2, uvRotate);
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

        public QuadSide GetQuadSide() => _quad;
    }
}
