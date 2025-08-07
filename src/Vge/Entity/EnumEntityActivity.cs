namespace Vge.Entity
{
    /// <summary>
    /// Перечень активности сущности, для анимации
    /// </summary>
    public enum EnumEntityActivity
    {
        /// <summary>
        /// Не используется, этого значения быть не должно
        /// </summary>
        None = 0,
        /// <summary>
        /// Стоячее положение без движения
        /// </summary>
        Idle = 1,
        /// <summary>
        /// Движется в любую из 8 сторон
        /// </summary>
        Move = 2,
        /// <summary>
        /// Движется вперёд
        /// </summary>
        Forward = 4,
        /// <summary>
        /// Движется назад
        /// </summary>
        Back = 8,
        /// <summary>
        /// Движется влево
        /// </summary>
        Left = 16,
        /// <summary>
        /// Движется вправо
        /// </summary>
        Right = 32,
        /// <summary>
        /// Крадётся
        /// </summary>
        Sneak = 64,
        /// <summary>
        /// Бежит
        /// </summary>
        Sprint = 128,
        /// <summary>
        /// В воздухе
        /// </summary>
        Levitate = 256,
        /// <summary>
        /// В воде или жидкости, параметр определяет мир
        /// </summary>
        Water = 512,
        /// <summary>
        /// Взбираться, рядом возле лестницы или подобного, параметр  определяет мир
        /// </summary>
        Climb = 1024,
        /// <summary>
        /// Смерть
        /// </summary>
        Dead = 2048
    }
}
