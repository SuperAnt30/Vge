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
        /// Объект рендера всех сущностей
        /// </summary>
        protected readonly EntitiesRenderer _entities;

        /// <summary>
        /// Объект для рендера слоёв (предметов или одежды) на сущности
        /// </summary>
        protected readonly EntityLayerRender _entityLayerRender;

        /// <summary>
        /// Ресурсы сущности, нужны везде, но форма только на клиенте
        /// </summary>
        protected readonly ResourcesEntity _resourcesEntity;

        public EntityRenderClient(EntityBase entity, EntitiesRenderer entities, ResourcesEntity resourcesEntity)
            : base(entity)
        {
            _entities = entities;
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
                _entityRender = _entities.GetEntityRender(Entity.IndexEntity);
            }
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public override void Draw(float timeIndex, float deltaTime)
        {
            // Заносим в шейдор
            _entities.ShsEntity.UniformData(
                Entity.GetPosFrameX(timeIndex) - _entities.Player.PosFrameX,
                Entity.GetPosFrameY(timeIndex) - _entities.Player.PosFrameY,
                Entity.GetPosFrameZ(timeIndex) - _entities.Player.PosFrameZ,
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
