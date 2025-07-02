using Vge.Entity.Model;

namespace Vge.Entity.Animation
{
    /// <summary>
    /// Класс, представляющий модель отдельного анимационного клипа (например, бег, прицеливание, ходьбу)
    /// </summary>
    public class ModelAnimationClip
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
        /// Длительность анимации, милисекунды
        /// </summary>
        public readonly float Duration;
        /// <summary>
        /// Время микса в милисекундах
        /// </summary>
        public readonly int TimeMix;
        /// <summary>
        /// Скорость клипа, 1 норма
        /// </summary>
        public readonly float Speed;
        /// <summary>
        /// Списки ключевых кадров для каждой кости скелета
        /// </summary>
        public readonly BoneAnimationChannel[] Bones;

        public ModelAnimationClip(string name, ModelLoop loop, float duration,
            int timeMix, float speed, BoneAnimationChannel[] bones)
        {
            Name = name;
            Loop = loop;
            TimeMix = timeMix;
            Speed = speed;
            Duration = duration;
            Bones = bones;
        }

        public override string ToString() => "Clip " + Name + " " + Duration;
    }
}
