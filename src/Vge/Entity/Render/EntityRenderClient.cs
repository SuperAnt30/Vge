using System.Runtime.CompilerServices;
using Vge.Renderer.World.Entity;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Объект рендера сущности, хранящий данные рендера, без анимации. Только для клиента
    /// </summary>
    public class EntityRenderClient : EntityRenderAbstract
    {
        /// <summary>
        /// Статичный объект сетки типа сущности, не меняется
        /// </summary>
        protected EntityRender _entityRender;

        /// <summary>
        /// Объект для рендера слоёв (предметов или одежды) на сущности
        /// </summary>
        protected readonly EntityLayerRender _entityLayerRender;

        /// <summary>
        /// Ресурсы сущности, нужны везде, но форма только на клиенте
        /// </summary>
        protected readonly ResourcesEntity _resourcesEntity;

        public EntityRenderClient(EntityBase entity, EntitiesRenderer entities, ResourcesEntity resourcesEntity)
            : base(entity, entities)
        {
            _resourcesEntity = resourcesEntity;

            // Если имеется инвентарь, то создаём объект слоёв
            if (Entity is EntityLiving || _resourcesEntity.IsAnimation) // Временно реагируем на живую сущность
            {
                _entityLayerRender = new EntityLayerRender(entities);
            }
        }

        /// <summary>
        /// Обновить рассчитать матрицы для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateMatrix(float timeIndex, float deltaTime)
        {
            if (_entityRender == null)
            {
                _entityRender = Entities.GetEntityRender(Entity.IndexEntity);
            }
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public override void Draw(float timeIndex, float deltaTime)
        {
            float ppfx = Entities.Player.PosFrameX;
            float ppfy = Entities.Player.PosFrameY;
            float ppfz = Entities.Player.PosFrameZ;

            // Заносим в шейдор
            Entities.ShsEntity.UniformData(
                Entity.GetPosFrameX(timeIndex) - ppfx,
                Entity.GetPosFrameY(timeIndex) - ppfy,
                Entity.GetPosFrameZ(timeIndex) - ppfz,
                _lightBlock, _lightSky,
                _resourcesEntity.GetDepthTextureAndSmall(),
                _resourcesEntity.GetIsAnimation(), 0
                );

            // Рисуем основную сетку сущности
            _entityRender.MeshDraw();

            // Если имеются слои, рисуем сетку слоёв
            _entityLayerRender?.MeshDraw();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Dispose() => _entityLayerRender?.Dispose();
    }
}
