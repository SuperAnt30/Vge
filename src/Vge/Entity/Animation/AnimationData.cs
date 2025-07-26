using System.Collections.Generic;

namespace Vge.Entity.Animation
{
    /// <summary>
    /// Анимационные данные, из json сущности
    /// </summary>
    public class AnimationData
    {
        /// <summary>
        /// Имя анимации
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Скорость клипа, 1 норма
        /// </summary>
        public readonly float Speed;
        /// <summary>
        /// Нахвание костей с весами, если имеются, если нет их вес по умолчанию
        /// </summary>
        public readonly Dictionary<string, byte> ElementWeight = new Dictionary<string, byte>();

        public AnimationData(string name, float speed)
        {
            Name = name;
            Speed = speed == 0 ? 1 : speed;
        }

        /// <summary>
        /// Получить весь кости
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
