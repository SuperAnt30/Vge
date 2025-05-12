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
        public readonly bool Loop;
        /// <summary>
        /// Длительность анимации, милисекунды
        /// </summary>
        public readonly float Duration;
        /// <summary>
        /// Списки ключевых кадров для каждой кости скелета
        /// </summary>
        public readonly BoneAnimationChannel[] Bones;

        public ModelAnimationClip(string name, bool loop, float duration, BoneAnimationChannel[] bones)
        {
            Name = name;
            Loop = loop;
            Duration = duration;
            Bones = bones;
        }

        public override string ToString() => "Clip " + Name + " " + Duration;
    }
}
