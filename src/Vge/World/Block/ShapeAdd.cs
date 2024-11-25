using Vge.Json;

namespace Vge.World.Block
{
    /// <summary>
    /// Дополнительные данные готовой фигуры
    /// </summary>
    public class ShapeAdd
    {
        /// <summary>
        /// Имеется ли смещение
        /// </summary>
        public bool IsOffset;
        /// <summary>
        /// Вращение по Y 0 | 90 | 180 | 270
        /// </summary>
        public int RotateY;
        /// <summary>
        /// При вращении RotateY сохраняется текстура, и не вращается сверху и снизу
        /// </summary>
        public bool UvLock;

        /// <summary>
        /// Смещение фигуры
        /// </summary>
        private float[] _offset = new float[0];

        public void RunShape(JsonCompound variant)
        {
            IsOffset = variant.IsKey(Ctb.Offset);
            if (IsOffset)
            {
                _offset = variant.GetArray(Ctb.Offset).ToArrayFloat();
            }
            else
            {
                _offset = new float[0];
            }
            // Имеется вращение по Y 90 | 180 | 270
            RotateY = _CheckRotate(variant.GetInt(Ctb.RotateY));
            
            // Защита от вращении текстуры
            UvLock = variant.GetBool(Ctb.UvLock);
        }

        public float GetOffsetX() => _offset.Length > 0 ? _offset[0] / 16f : 0;
        public float GetOffsetY() => _offset.Length > 1 ? _offset[1] / 16f : 0;
        public float GetOffsetZ() => _offset.Length > 2 ? _offset[2] / 16f : 0;

        /// <summary>
        /// Проверка вращения кратно 90 || 180 || 270
        /// </summary>
        private int _CheckRotate(int rotate)
            => (rotate == 90 || rotate == 180 || rotate == 270) ? rotate : 0;
    }
}
