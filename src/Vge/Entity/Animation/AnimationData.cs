namespace Vge.Entity.Animation
{
    /// <summary>
    /// Анимационные данные, из json сущности
    /// </summary>
    public readonly struct AnimationData
    {
        /// <summary>
        /// Имя анимации
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Скорость клипа, 1 норма
        /// </summary>
        public readonly float Speed;

        public AnimationData(string name, float speed)
        {
            Name = name;
            Speed = speed == 0 ? 1 : speed;
        }

    }
}
