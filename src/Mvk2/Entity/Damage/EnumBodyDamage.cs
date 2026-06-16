namespace Mvk2.Entity.Damage
{
    /// <summary>
    /// Перечень расположения урона по телу
    /// </summary>
    public enum EnumBodyDamage
    {
        /// <summary>
        /// Удар по всему телу, но без урона по одежде
        /// </summary>
        Null = 0,
        /// <summary>
        /// Голова
        /// </summary>
        Head = 1,
        /// <summary>
        /// Тело
        /// </summary>
        Body = 2,
        /// <summary>
        /// Ноги
        /// </summary>
        Legs = 3,
        /// <summary>
        /// Стопы
        /// </summary>
        Feet = 4,
        /// <summary>
        /// Все
        /// </summary>
        All = 5
    }
}
