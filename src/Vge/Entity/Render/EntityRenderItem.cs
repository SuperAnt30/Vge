using System.Runtime.CompilerServices;
using Vge.Entity.List;
using Vge.Item;
using Vge.Renderer.World.Entity;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Объект рендера сущности, хранящий данные рендера, без анимации. Только для клиента
    /// </summary>
    public class EntityRenderItem : EntityRenderAbstract
    {
        /// <summary>
        /// Объект рендера всех сущностей
        /// </summary>
        protected readonly EntitiesRenderer _entities;

        public EntityRenderItem(EntityBase entity, EntitiesRenderer entities)
            : base(entity) => _entities = entities;

        /// <summary>
        /// Обновить рассчитать матрицы для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UpdateMatrix(float timeIndex, float deltaTime)
        {
            if (_entityRender == null && Entity is EntityItem entityItem)
            {
                ItemStack itemStack = entityItem.GetEntityItemStack();
                if (itemStack != null)
                {
                    _entityRender = _entities.GetItemRender(itemStack.Item.IndexItem);
                }
            }
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public override void Draw(float timeIndex, float deltaTime)
        {
            if (_entityRender != null) // Может быть случай, когда пакет данных стака ещё не пришёл
            {
                // Заносим в шейдор
                _entities.ShsEntity.UniformData(
                    Entity.GetPosFrameX(timeIndex) - _entities.Player.PosFrameX,
                    Entity.GetPosFrameY(timeIndex) - _entities.Player.PosFrameY,
                    Entity.GetPosFrameZ(timeIndex) - _entities.Player.PosFrameZ,
                    _lightBlock, _lightSky);

                // Рисуем основную сетку сущности
                _entityRender.MeshDraw();
            }
        }
    }
}
