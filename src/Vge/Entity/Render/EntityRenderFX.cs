using Vge.Renderer.World.Entity;
using Vge.World;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Объект рендера эффектов, рендера, только для клиента
    /// </summary>
    public class EntityRenderFX : EntityRenderBase
    {
        /// <summary>
        /// Объект рендера всех сущностей
        /// </summary>
        public readonly EntitiesRenderer Entities;

        public EntityRenderFX(EntityBase entity, EntitiesRenderer entities) : base(entity)
        {
            Entities = entities;
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public override void Draw(float timeIndex, float deltaTime) { }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime) { }
    }
}
