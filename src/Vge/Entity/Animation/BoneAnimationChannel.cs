namespace Vge.Entity.Animation
{
    /// <summary>
    /// Объект анимационного конкретного клипа с набором ключевых кадров отдельной кости.
    /// </summary>
    public class BoneAnimationChannel
    {
        /// <summary>
        /// Имеется ли у этой кости анимация
        /// </summary>
        public readonly bool IsAnimation;
        /// <summary>
        /// Вес кости в момент смешивания
        /// </summary>
        public readonly byte Weight;
        /// <summary>
        /// Массив кадров с позицией кости
        /// </summary>
        public readonly BoneAnimationFrame[] PositionFrames;
        /// <summary>
        /// Массив кадров с ориентацией кости
        /// </summary>
        public readonly BoneAnimationFrame[] OrientationFrames;

        public BoneAnimationChannel(byte weight,
            BoneAnimationFrame[] positionFrames,
            BoneAnimationFrame[] orientationFrames)
        {
            Weight = weight;
            PositionFrames = positionFrames;
            OrientationFrames = orientationFrames;
            IsAnimation = positionFrames.Length != 0 || orientationFrames.Length != 0;
        }

        public override string ToString() 
            => PositionFrames.Length.ToString() + " " + OrientationFrames.Length.ToString();
    }
}
