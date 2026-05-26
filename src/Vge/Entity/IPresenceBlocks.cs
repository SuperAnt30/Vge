using Vge.World;

namespace Vge.Entity
{
    /// <summary>
    /// Интерфейс наличия блоков в которой находится сущность
    /// </summary>
    public interface IPresenceBlocks
    {
        /// <summary>
        /// Находится ли в воде
        /// </summary>
        bool IsInWater { get; }

        /// <summary>
        /// Находится ли в любой из жидкостей
        /// </summary>
        bool IsInLiquid { get; }
        /// <summary>
        /// Ускорение всплытия в жидкости
        /// </summary>
        float AccelerationAscentInLiquid { get; }
        /// <summary>
        /// Коэффициент трения в жидкости
        /// </summary>
        float FactorInertiaInLiquid { get; }
        /// <summary>
        /// Коэффициент гравитации в жидкости
        /// </summary>
        float FactorGravityInLiquid { get; }

        /// <summary>
        /// Имеются ли замедления
        /// </summary>
        bool IsSlow { get; }
        /// <summary>
        /// Коэффициент замедления по X и Z
        /// </summary>
        float FactorSlowXZ { get; }
        /// <summary>
        /// Коэффициент замедления по Y
        /// </summary>
        float FactorSlowY { get; }

        /// <summary>
        /// Имеется импульс
        /// </summary>
        bool IsImpulse { get; }
        /// <summary>
        /// Импульс по координате X
        /// </summary>
        float ImpulseX { get; }
        /// <summary>
        /// Импульс по координате X
        /// </summary>
        float ImpulseY { get; }
        /// <summary>
        /// Импульс по координате X
        /// </summary>
        float ImpulseZ { get; }

        /// <summary>
        /// Обновление перерасчёта в такте
        /// </summary>
        void Update(WorldBase world);
    }
}
