using System.Collections.Generic;

namespace Vge.Entity.Model
{
    /// <summary>
    /// Объект куба части моба для скелетной анимации
    /// </summary>
    public class ModelCube
    {
        /// <summary>
        /// Идентификационный номер для определения дерева, из Blockbench
        /// </summary>
        public readonly string Uuid;
        /// <summary>
        /// Имя куба из Blockbench
        /// </summary>
        public readonly string Name;
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
        /// Вращение по X
        /// </summary>
        public float RotationX;
        /// <summary>
        /// Вращение по Y
        /// </summary>
        public float RotationY;
        /// <summary>
        /// Вращение по Z
        /// </summary>
        public float RotationZ;

        /// <summary>
        /// Центральная точка для вращения по X
        /// </summary>
        public float OriginX;
        /// <summary>
        /// Центральная точка для вращения по Y
        /// </summary>
        public float OriginY;
        /// <summary>
        /// Центральная точка для вращения по Z
        /// </summary>
        public float OriginZ;

        /// <summary>
        /// Ширина
        /// </summary>
        public readonly int Width;
        /// <summary>
        /// Высота
        /// </summary>
        public readonly int Height;

        public ModelCube(string uuid, string name, int width, int height)
        {
            Uuid = uuid;
            Name = name;
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
        /// Повернуть куб если это необходимо
        /// </summary>
        public void SetRotation(float[] rotation, float[] origin)
        {
            RotationX = rotation[0];
            RotationY = rotation[1];
            RotationZ = rotation[2];

            OriginX = origin[0];
            OriginY = origin[1];
            OriginZ = origin[2];
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

        public override string ToString() => Name + " " + Uuid;
    }
}
