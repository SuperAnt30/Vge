using System.Collections.Generic;

namespace Vge.Entity.Animation
{
    /// <summary>
    /// Анимационные данные, из json сущности
    /// </summary>
    public class AnimationData
    {
        /// <summary>
        /// Имя клипа как ключ для программы, должны быть уникальны в одной сущности
        /// </summary>
        public readonly string Code;
        /// <summary>
        /// Имя клипа в Blockbanche
        /// </summary>
        public readonly string Animation;
        /// <summary>
        /// Скорость клипа, 1 норма
        /// </summary>
        public readonly float Speed;
        /// <summary>
        /// Название тригеров для активации текущей анимации
        /// </summary>
        public readonly string[] TriggeredBy;
        /// <summary>
        /// Название костей с весами, если имеются, если нет их вес по умолчанию
        /// </summary>
        public readonly Dictionary<string, byte> ElementWeight = new Dictionary<string, byte>();

        public AnimationData(string code, string animation, float speed, string[] triggeredBy)
        {
            Code = code;
            Animation = animation;
            Speed = speed == 0 ? 1 : speed;
            TriggeredBy = triggeredBy;
        }

        /// <summary>
        /// Получить вес кости
        /// </summary>
        public byte GetWeight(string nameBone)
        {
            if (ElementWeight.ContainsKey(nameBone))
            {
                return ElementWeight[nameBone];
            }
            // Значение по умолчанию
            return 0;
        }
    }
}
