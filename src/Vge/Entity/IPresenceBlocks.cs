namespace Vge.Entity
{
    /// <summary>
    /// Интерфейс наличия блоков в которой находится сущность
    /// </summary>
    public interface IPresenceBlocks
    {
        /// <summary>
        /// Обновление перерасчёта в такте
        /// </summary>
        void Update();

        /// <summary>
        /// Находится ли в воде
        /// </summary>
        bool IsInWater();

        /// <summary>
        /// Находится ли в любой из жидкостей
        /// </summary>
        bool IsInLiquid();
    }
}
