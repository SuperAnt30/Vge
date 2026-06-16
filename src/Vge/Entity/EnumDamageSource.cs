namespace Vge.Entity
{
    /// <summary>
    /// Перечень базовых источников урона
    /// </summary>
    public enum EnumDamageSource
    {
        /// <summary>
        /// Вне мира
        /// </summary>
        OutOfWorld = 0,
        /// <summary>
        /// Падение
        /// </summary>
        Fall = 1,
        /// <summary>
        /// В огне, когда зашёл в него
        /// </summary>
        InFire = 2,
        /// <summary>
        /// От огня, когда долго горел из-за огня
        /// </summary>
        OnFire = 3,
        /// <summary>
        /// Лава
        /// </summary>
        Lava = 4,
        /// <summary>
        /// Тонем
        /// </summary>
        Drown = 5,
        /// <summary>
        /// Игрок
        /// </summary>
        Player = 6,


        ///// <summary>
        ///// В стене
        ///// </summary>
        //InWall,
        ///// <summary>
        ///// Соприкосновение с кактусом
        ///// </summary>
        //Cactus,
        ///// <summary>
        ///// Источник взрыва
        ///// </summary>
        //ExplosionSource,
        ///// <summary>
        ///// Кусочек
        ///// </summary>
        //Piece,
        
        ///// <summary>
        ///// Нанести урон мобом
        ///// </summary>
        //CauseMobDamage,
        ///// <summary>
        ///// Принудительная смерть от команды
        ///// </summary>
        //Kill,
        ///// <summary>
        ///// Заноза в кровате
        ///// </summary>
        //Splinter,
        ///// <summary>
        ///// Мушкет
        ///// </summary>
        //Musket
    }
}
