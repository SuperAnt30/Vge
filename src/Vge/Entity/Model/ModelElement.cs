namespace Vge.Entity.Model
{
    /// <summary>
    /// Объект элемента части модели
    /// </summary>
    public abstract class ModelElement
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
        /// Масштаб
        /// </summary>
        public readonly float Scale;
        /// <summary>
        /// Индекс кости
        /// </summary>
        public byte BoneIndex;

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

        public ModelElement(string uuid, string name, float scale)
        {
            Uuid = uuid;
            Name = name;
            Scale = scale;
        }

        /// <summary>
        /// Повернуть куб если это необходимо
        /// </summary>
        public void SetRotation(float[] rotation, float[] origin)
        {
            RotationX = rotation[0];
            RotationY = rotation[1];
            RotationZ = rotation[2];

            OriginX = origin[0] * Scale;
            OriginY = origin[1] * Scale;
            OriginZ = origin[2] * Scale;
        }

        public override string ToString() => BoneIndex + " " + Name + " " + Uuid;
    }
}
