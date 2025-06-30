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
        /// Начальное время микса в милисекундах
        /// </summary>
        public readonly int TimeMixBegin;
        /// <summary>
        /// Конечное время микса в милисекундах
        /// </summary>
        public readonly int TimeMixEnd;
        /// <summary>
        /// Скорость клипа, 1 норма
        /// </summary>
        public readonly float Speed;

        public AnimationData(string name, int timeMixBegin, int timeMixEnd, float speed)
        {
            Name = name;
            TimeMixBegin = timeMixBegin;
            TimeMixEnd = timeMixEnd;
            Speed = speed == 0 ? 1 : speed;
        }

    }
}
