using Vge.Entity.Model;

namespace Vge.Entity.Animation
{
    /// <summary>
    /// Объект анимационного конкретного клипа с набором ключевых кадров отдельной кости.
    /// </summary>
    public class BoneAnimationChannel
    {
        /// <summary>
        /// Имя анимации из Blockbench
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Анимация без перерыва, цикл
        /// </summary>
        public readonly ModelLoop Loop;
        /// <summary>
        /// Имеется ли у этой кости анимация
        /// </summary>
        public readonly bool IsAnimation;
        /// <summary>
        /// Массив кадров с позицией кости
        /// </summary>
        public readonly BoneAnimationFrame[] PositionFrames;
        /// <summary>
        /// Массив кадров с ориентацией кости
        /// </summary>
        public readonly BoneAnimationFrame[] OrientationFrames;

        public BoneAnimationChannel(string name, ModelLoop loop,
            BoneAnimationFrame[] positionFrames,
            BoneAnimationFrame[] orientationFrames)
        {
            Name = name;
            Loop = loop;
            PositionFrames = positionFrames;
            OrientationFrames = orientationFrames;
            IsAnimation = positionFrames.Length != 0 || orientationFrames.Length != 0;
        }

        public override string ToString() 
            => PositionFrames.Length.ToString() + " " + OrientationFrames.Length.ToString();
    }
}
