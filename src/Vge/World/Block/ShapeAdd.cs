using Vge.Json;

namespace Vge.World.Block
{
    /// <summary>
    /// Дополнительные данные готовой фигуры
    /// </summary>
    public class ShapeAdd
    {
        /// <summary>
        /// Имеется ли масштаб
        /// </summary>
        public bool IsScale;
        /// <summary>
        /// Масштаб 1.0 == 100%
        /// </summary>
        public float Scale = 1f;

        /// <summary>
        /// Имеется ли смещение
        /// </summary>
        public bool IsOffset;
        /// <summary>
        /// Смещение фигуры
        /// </summary>
        public float[] Offset = new float[0];

        /// <summary>
        /// Имеется ли вращение, только для предмета
        /// </summary>
        public bool IsRotation;
        /// <summary>
        /// фращение фигуры
        /// </summary>
        public float[] Rotate;

        /// <summary>
        /// Вращение по Y 0 | 90 | 180 | 270
        /// Только для блока
        /// </summary>
        public int RotateY;
        /// <summary>
        /// При вращении RotateY сохраняется текстура, и не вращается сверху и снизу
        /// Только для блока
        /// </summary>
        public bool UvLock;

        public void RunShape(JsonCompound view, bool isBlock, int sizeSprite)
        {
            if (view.Items != null)
            {
                IsScale = view.IsKey(Ctb.Scale);
                if (IsScale)
                {
                    Scale = view.GetFloat(Ctb.Scale);
                    if (sizeSprite != 1)
                    {
                        Scale *= sizeSprite;
                    }
                }
                else if (sizeSprite != 1)
                {
                    IsScale = true;
                    Scale = sizeSprite;
                }

                    IsOffset = view.IsKey(Ctb.Offset);
                if (IsOffset)
                {
                    Offset = view.GetArray(Ctb.Offset).ToArrayFloat();
                }
                else
                {
                    Offset = new float[3];
                }

                // Если блок, смещаем на край блока, 0 .. +16 чтоб был.
                // А был до этого -8 .. +8
                if (isBlock)
                {
                    Offset[0] += 8f;
                    Offset[2] += 8f;
                    IsOffset = Offset[0] != 0 || Offset[1] != 0 || Offset[2] != 0;

                    // Имеется вращение по Y 90 | 180 | 270
                    RotateY = _CheckRotate(view.GetInt(Ctb.RotateY));

                    // Защита от вращении текстуры
                    UvLock = view.GetBool(Ctb.UvLock);
                }
                else
                {
                    IsRotation = view.IsKey(Ctb.Rotate);
                    if (IsRotation)
                    {
                        Rotate = view.GetArray(Ctb.Rotate).ToArrayFloat();
                    }
                }

                if (IsOffset && sizeSprite == 1) // Это не спрайт для GUI
                {
                    Offset[0] /= 16f;
                    Offset[1] /= 16f;
                    Offset[2] /= 16f;
                }
            }
            else if(sizeSprite != 1)
            {
                IsScale = true;
                Scale = sizeSprite;
            }
        }

        /// <summary>
        /// Проверка вращения кратно 90 || 180 || 270
        /// </summary>
        private int _CheckRotate(int rotate)
            => (rotate == 90 || rotate == 180 || rotate == 270) ? rotate : 0;
    }
}
