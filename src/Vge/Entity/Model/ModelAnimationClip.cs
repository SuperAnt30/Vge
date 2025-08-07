using System;
using Vge.Entity.Model;

namespace Vge.Entity.Animation
{
    /// <summary>
    /// Класс, представляющий модель отдельного анимационного клипа (например, бег, прицеливание, ходьбу)
    /// </summary>
    public class ModelAnimationClip
    {
        /// <summary>
        /// Имя клипа как ключ для программы, должны быть уникальны в одной сущности
        /// </summary>
        public readonly string Code;
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
        /// Перечень триггеров для активации текущей анимации
        /// </summary>
        public readonly EnumEntityActivity Activity;
        /// <summary>
        /// Списки ключевых кадров для каждой кости скелета
        /// </summary>
        public readonly BoneAnimationChannel[] Bones;

        public ModelAnimationClip(string entityName, string code, ModelLoop loop, float duration,
            int timeMix, AnimationData animationData, BoneAnimationChannel[] bones)
        {
            Code = code;
            Loop = loop;
            TimeMix = timeMix;
            Speed = animationData.Speed;
            Activity = EnumEntityActivity.None;
            for (int i = 0; i < animationData.TriggeredBy.Length; i++)
            {
                Enum.TryParse(animationData.TriggeredBy[i], out EnumEntityActivity activity);
                if (activity == EnumEntityActivity.None)
                {
                    throw new Exception(Sr.GetString(Sr.InvalidTriggerInAnimationEntity,
                        animationData.TriggeredBy[i], Code, entityName));
                }
                Activity |= activity;
            }
            Duration = duration;
            Bones = bones;
        }

        public override string ToString() => "Clip " + Code + " " + Duration;
    }
}
