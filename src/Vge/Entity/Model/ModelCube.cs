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
        /// Покуда не используется. 10.07.2025
        /// </summary>
        public byte Index;
        /// <summary>
        /// Является ли этот куб одеждой
        /// </summary>
        public bool Layer;

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
        /// <summary>
        /// Видимость куба
        /// </summary>
        public readonly bool Visible;
        /// <summary>
        /// Тип блока глаз 0-куб, 1-глаза закрыты, //2-глаза открыты, //3-рот закрыт, 4-рот открыт, 5-рот улыбка 
        /// </summary>
        public readonly byte EyeMouth; 

        public ModelCube(string uuid, string name, int width, int height, bool visible) : base(uuid, name)
        {
            Width = width;
            Height = height;
            switch(name)
            {
                case ModelEntityDefinition.NameCubeEyeClose: EyeMouth = 1; Visible = true; break;
                case ModelEntityDefinition.NameCubeMouthOpen: EyeMouth = 4; Visible = true; break;
                case ModelEntityDefinition.NameCubeMouthSmile: EyeMouth = 5; Visible = true; break;
                default: Visible = visible; break;
            }
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
        public void GenBuffer(List<float> bufferFloat, List<int> bufferInt)
        {
            for (int i = 0; i < 6; i++)
            {
                Faces[i].GenBuffer(bufferFloat, bufferInt, this);
            }
        }

        public override string ToString()
            => Index.ToString() + " " + base.ToString();
    }
}
