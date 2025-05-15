using System.Collections.Generic;

namespace Vge.Entity.Model
{
    /// <summary>
    /// Объект куба части моба для скелетной анимации
    /// </summary>
    public class ModelCube : ModelElement
    {
        /// <summary>
        /// Индекс очерёдности куба в Blockbanch
        /// </summary>
        public byte Index;

        /// <summary>
        /// Количество сторон
        /// </summary>
        public readonly ModelFace[] Faces = new ModelFace[6];

        /// <summary>
        /// Позиция от X
        /// </summary>
        public float FromX;
        /// <summary>
        /// Позиция от Y
        /// </summary>
        public float FromY;
        /// <summary>
        /// Позиция от Z
        /// </summary>
        public float FromZ;

        /// <summary>
        /// Позиция до X
        /// </summary>
        public float ToX;
        /// <summary>
        /// Позиция до Y
        /// </summary>
        public float ToY;
        /// <summary>
        /// Позиция до Z
        /// </summary>
        public float ToZ;

        /// <summary>
        /// Ширина
        /// </summary>
        public readonly int Width;
        /// <summary>
        /// Высота
        /// </summary>
        public readonly int Height;

        public ModelCube(string uuid, string name, int width, int height) : base(uuid, name)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Установить размер и расположение куба
        /// </summary>
        public void SetPosition(float[] from, float[] to)
        {
            FromX = from[0];
            FromY = from[1];
            FromZ = from[2];

            ToX = to[0];
            ToY = to[1];
            ToZ = to[2];
        }

        /// <summary>
        /// Сгенерировать буффер
        /// </summary>
        public void GenBuffer(List<float> buffer)
        {
            for (int i = 0; i < 6; i++)
            {
                Faces[i].GenBuffer(buffer, this);
            }
        }

        public override string ToString()
            => Index.ToString() + " " + base.ToString();
    }
}
