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

        public AnimationData(string name, int timeMixBegin, int timeMixEnd)
        {
            Name = name;
            TimeMixBegin = timeMixBegin;
            TimeMixEnd = timeMixEnd;
        }

    }
}
