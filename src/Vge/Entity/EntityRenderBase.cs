using Vge.World;

namespace Vge.Entity
{
    /// <summary>
    /// Базовый класс рендера для сущности, объект пустой, для сервера
    /// </summary>
    public class EntityRenderBase
    {
        /// <summary>
        /// Сущность к которой прекреплена физика
        /// </summary>
        public readonly EntityBase Entity;

        public EntityRenderBase(EntityBase entity) => Entity = entity;

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public virtual void Draw(float timeIndex, float deltaTime) { }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        public virtual void UpdateClient(WorldClient world) { }
    }
}
